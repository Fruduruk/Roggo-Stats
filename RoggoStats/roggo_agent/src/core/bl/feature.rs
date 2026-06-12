use std::collections::HashSet;
use std::path::Path;

use uuid::Uuid;

use crate::AGENT_VERSION;
use crate::core::api::contract::{
    DetailedAverageAdvancedStatsDto, DetailedAverageCoreStatsDto, DetailedAveragePlayerDto,
    DetailedMatchDto, DetailedPlayerDto, DetailedPlayerStatsDto, DetailedSessionDto,
    DetailedTeamDto, MVPType, MainCharacterDto, SessionMatchDto, SimpleMatchDto, SimpleSessionDto,
    VersionDto,
};
use crate::core::bl::query_models::{F3PlayerRow, F3TeamRow, GlobalPlayerRow};
use crate::core::bl::{Error, Result};
use crate::core::db::Repository;

pub fn get_version() -> VersionDto {
    VersionDto {
        version: AGENT_VERSION.into(),
    }
}

fn get_most_played_player(repo: &Repository) -> Result<GlobalPlayerRow> {
    let global_player = repo
        .get_player_with_most_replays()
        .map_err(|err| Error::NoPlayerFound { source: err })?;
    Ok(global_player)
}

pub fn hide_match(path: &Path, match_guid: Uuid, hide: bool) -> Result<()> {
    let repo = Repository::connect(path)?;

    repo.hide_match(match_guid, hide)?;

    Ok(())
}

pub fn get_detailed_session(path: &Path, match_guids: Vec<Uuid>) -> Result<DetailedSessionDto> {
    let repo = Repository::connect(path)?;

    let main_character = get_most_played_player(&repo)?;

    let mut seen = HashSet::new();

    let match_guids: Vec<_> = match_guids
        .into_iter()
        .filter(|guid| seen.insert(*guid))
        .collect();

    let own_team_player_averages = repo
        .f5_get_own_team_player_averages(match_guids.clone(), main_character.id)?
        .into_iter()
        .map(|row| {
            Ok(DetailedAveragePlayerDto {
                username: row.last_username,
                average_core_stats: DetailedAverageCoreStatsDto::from_options(
                    row.average_core_stats.average_score,
                    row.average_core_stats.average_goals,
                    row.average_core_stats.average_shots,
                    row.average_core_stats.average_assists,
                    row.average_core_stats.average_saves,
                    row.average_core_stats.average_demos,
                )
                .ok_or(Error::CalculationError(
                    "Core stats should be available here".into(),
                ))?,
                average_advanced_stats: DetailedAverageAdvancedStatsDto::from_options(
                    row.average_advanced_stats.average_percent_boosting,
                    row.average_advanced_stats.average_percent_demolished,
                    row.average_advanced_stats.average_percent_on_ground,
                    row.average_advanced_stats.average_percent_on_wall,
                    row.average_advanced_stats.average_percent_powersliding,
                    row.average_advanced_stats.average_percent_supersonic,
                ),
            })
        })
        .collect::<Result<Vec<_>>>()?;

    let row = repo.f5_get_enemy_player_core_averages(match_guids.clone(), main_character.id)?;
    let average_enemy_core_stats = DetailedAverageCoreStatsDto::from_options(
        row.average_score,
        row.average_goals,
        row.average_shots,
        row.average_assists,
        row.average_saves,
        row.average_demos,
    );

    let row = repo.f5_get_team_player_core_averages(match_guids.clone(), main_character.id)?;
    let average_team_player_core_stats = DetailedAverageCoreStatsDto::from_options(
        row.average_score,
        row.average_goals,
        row.average_shots,
        row.average_assists,
        row.average_saves,
        row.average_demos,
    );

    let row = repo.f5_get_team_player_advanced_averages(match_guids.clone(), main_character.id)?;
    let average_team_player_advanced_stats = DetailedAverageAdvancedStatsDto::from_options(
        row.average_percent_boosting,
        row.average_percent_demolished,
        row.average_percent_on_ground,
        row.average_percent_on_wall,
        row.average_percent_powersliding,
        row.average_percent_supersonic,
    );

    let session_matches = repo
        .f5_get_session_matches(match_guids.clone(), main_character.id)?
        .into_iter()
        .map(|row| {
            let mvp_type = if row.own_best_global_player_id == main_character.id {
                if row.main_character_won.unwrap_or(true) {
                    MVPType::MVP
                } else {
                    MVPType::ACE
                }
            } else {
                MVPType::Nothing
            };

            SessionMatchDto {
                match_guid: row.match_guid,
                created_at: row.created_at,
                ended_at: row.ended_at,
                won: row.main_character_won,
                mvp_type,
                hidden: row.hidden,
            }
        })
        .collect();

    let dto = DetailedSessionDto {
        session_matches,
        own_team_player_averages,
        average_enemy_core_stats,
        average_team_player_core_stats,
        average_team_player_advanced_stats,
    };

    Ok(dto)
}

pub fn get_all_sessions(path: &Path, pause_ms: i64) -> Result<Vec<SimpleSessionDto>> {
    let repo = Repository::connect(path)?;

    let main_character = get_most_played_player(&repo)?;

    let mut dtos: Vec<SimpleSessionDto> = vec![];

    for match_row in repo.f4_get_matches()? {
        let teams = repo.f2_get_teams_by_match_id(match_row.id)?;
        let mut own_teams_players = None;
        let mut enemy_teams_players = None;

        for team in teams {
            let players = repo.f2_get_players_by_team_id(team.id)?;
            let own_team = is_main_character_team(
                &main_character,
                players.iter().map(|p| p.global_player_id).collect(),
            );

            if own_team {
                own_teams_players = Some((team, players));
            } else {
                enemy_teams_players = Some((team, players));
            }
        }

        if let (Some((own_team, own_players)), Some((enemy_team, enemy_players))) =
            (&own_teams_players, &enemy_teams_players)
        {
            if own_team.score == enemy_team.score || own_players.len() != enemy_players.len() {
                continue;
            }
            let won = own_team.score > enemy_team.score;

            let mut added = false;

            for session in &mut dtos {
                if session.ended_at + pause_ms < match_row.ended_at {
                    continue;
                }
                if session.own_player_count != own_players.len() as i64
                    || session.enemy_player_count != enemy_players.len() as i64
                {
                    continue;
                }

                session.ended_at = match_row.ended_at;
                if !match_row.hidden {
                    session.match_count += 1;
                    if won {
                        session.matches_won += 1;
                    }
                }
                session.match_guids.push(match_row.match_guid);

                added = true;
                break;
            }

            if !added {
                let dto = SimpleSessionDto {
                    created_at: match_row.created_at,
                    ended_at: match_row.ended_at,
                    match_count: if match_row.hidden { 0 } else { 1 },
                    matches_won: if match_row.hidden {
                        0
                    } else {
                        if won { 1 } else { 0 }
                    },
                    match_guids: vec![match_row.match_guid],
                    own_player_count: own_players.len() as i64,
                    enemy_player_count: enemy_players.len() as i64,
                };
                dtos.push(dto);
            }
        } else {
            continue;
        }
    }

    Ok(dtos)
}

pub fn get_all_matches(path: &Path) -> Result<Vec<SimpleMatchDto>> {
    let repo = Repository::connect(path)?;

    let main_character = get_most_played_player(&repo)?;

    let mut dtos = vec![];

    for match_row in repo.f2_get_matches()? {
        let teams = repo.f2_get_teams_by_match_id(match_row.id)?;

        let mut teams_players = vec![];
        for team in teams {
            let players = repo.f2_get_players_by_team_id(team.id)?;
            teams_players.push((team, players));
        }

        let (
            mut own_team_score,
            mut enemy_team_score,
            mut own_player_count,
            mut enemy_player_count,
        ) = Default::default();

        for (team, players) in teams_players {
            let own_team = is_main_character_team(
                &main_character,
                players.iter().map(|p| p.global_player_id).collect(),
            );

            if own_team {
                own_team_score = team.score;
                own_player_count = players.len() as i64;
            } else {
                enemy_team_score = team.score;
                enemy_player_count = players.len() as i64;
            }
        }

        if own_player_count != enemy_player_count || own_team_score == enemy_team_score {
            continue;
        }

        dtos.push(SimpleMatchDto {
            match_guid: match_row.match_guid,
            duration: match_row.duration,
            ended_at: match_row.ended_at,
            hidden: match_row.hidden,
            own_team_score,
            enemy_team_score,
            own_player_count,
            enemy_player_count,
        })
    }

    Ok(dtos)
}

#[inline]
fn is_main_character_team(main_character: &GlobalPlayerRow, player_ids: Vec<i64>) -> bool {
    for id in player_ids {
        if id == main_character.id {
            return true;
        }
    }
    false
}

pub fn get_main_character(path: &Path) -> Result<MainCharacterDto> {
    let repo = Repository::connect(path)?;
    let main_character = get_most_played_player(&repo)?;

    Ok(MainCharacterDto {
        username: main_character.last_username,
        primary_id: main_character.primary_id,
    })
}

pub fn get_detailed_match_by_id(path: &Path, match_guid: Uuid) -> Result<DetailedMatchDto> {
    let repo = Repository::connect(path)?;
    let main_character = get_most_played_player(&repo)?;

    let match_row = repo.f3_get_match_by_match_guid(match_guid)?;
    let teams = repo.f3_get_teams_by_match_id(match_row.id)?;

    let mut own_team = None;
    let mut own_players = None;
    let mut enemy_team = None;
    let mut enemy_players = None;

    for team in teams {
        let players = repo.f3_get_players_by_team_id(team.id)?;

        if is_main_character_team(
            &main_character,
            players
                .iter()
                .map(|p| p.global_player_id)
                .collect::<Vec<_>>(),
        ) {
            own_team = Some(team);
            own_players = Some(players);
        } else {
            enemy_team = Some(team);
            enemy_players = Some(players);
        }
    }

    let own_team = own_team.ok_or(Error::CalculationError("own team not found".into()))?;
    let own_players = own_players.ok_or(Error::CalculationError("own players not found".into()))?;

    let enemy_team = enemy_team.ok_or(Error::CalculationError("enemy team not found".into()))?;
    let enemy_players =
        enemy_players.ok_or(Error::CalculationError("enemy players not found".into()))?;

    let duration = match_row.duration as f64;

    let own_team = convert_to_detailed_team(&repo, own_team, own_players, duration)?;
    let enemy_team = convert_to_detailed_team(&repo, enemy_team, enemy_players, duration)?;

    Ok(DetailedMatchDto {
        match_guid: match_guid,
        arena: match_row.arena,
        duration: match_row.duration,
        created_at: match_row.created_at,
        ended_at: match_row.ended_at,
        had_overtime: match_row.had_overtime,
        own_team,
        enemy_team,
        hidden: match_row.hidden,
    })
}

fn convert_to_detailed_team(
    repo: &Repository,
    team: F3TeamRow,
    players: Vec<F3PlayerRow>,
    duration: f64,
) -> Result<DetailedTeamDto> {
    let players: Vec<DetailedPlayerDto> = players
        .into_iter()
        .map(|p| {
            let username = repo
                .get_global_player_by_id(p.global_player_id)?
                .last_username;

            let stats = match p.stats {
                Some(stats) => Some(DetailedPlayerStatsDto {
                    percent_boosting: stats.time_boosting as f64 / duration,
                    percent_demolished: stats.time_demolished as f64 / duration,
                    percent_on_ground: stats.time_on_ground as f64 / duration,
                    percent_on_wall: stats.time_on_wall as f64 / duration,
                    percent_powersliding: stats.time_powersliding as f64 / duration,
                    percent_supersonic: stats.time_supersonic as f64 / duration,
                }),
                None => None,
            };

            Ok(DetailedPlayerDto {
                username,
                display_name: p.display_name,
                score: p.score,
                goals: p.goals,
                shots: p.shots,
                assists: p.assists,
                saves: p.saves,
                demos: p.demos,
                stats,
            })
        })
        .collect::<Result<Vec<_>>>()?;

    let team = DetailedTeamDto {
        name: team.name,
        score: team.score,
        color_primary: team.color_primary,
        color_secondary: team.color_secondary,
        players,
    };
    Ok(team)
}
