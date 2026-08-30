use std::collections::{BTreeSet, HashSet};
use std::path::Path;

use jiff::ToSpan;
use jiff::civil::Date;
use jiff::tz::TimeZone;
use uuid::Uuid;

use crate::AGENT_VERSION;
use crate::core::api::contract::{DayDto, DayMatchDto, DaySessionDto, PlayerDto, SessionTypeDto};
use crate::core::bl::features::{get_most_played_player, is_main_character_team};
use crate::core::bl::{Error, Result};
use crate::core::db::repository_features::day::get_teams_by_match_id;
use crate::core::db::repository_features::day::{DayMatchRow, PlayerRow, get_players_by_team_id};
use crate::core::db::{Repository, repository_features};

const SESSION_PAUSE_MS: i64 = 60 * 60 * 1000;

struct MatchWithTeams {
    match_row: DayMatchRow,
    won: bool,
    own_team: Team,
    enemy_team: Team,
}

struct Team {
    id: i64,
    score: i64,
    players: Vec<Player>,
}

struct Player {
    primary_id: String,
    display_name: String,
}

pub fn get(path: &Path, day: Date) -> Result<DayDto> {
    let repo = Repository::connect(path)?;

    let main_character = get_most_played_player(&repo)?;

    let filled_matches = get_full_matches(day, repo, main_character)?;

    let sessions = create_sessions(filled_matches);

    Ok(DayDto { sessions })
}

fn get_full_matches(
    day: Date,
    repo: Repository,
    main_character: crate::core::bl::query_models::GlobalPlayerRow,
) -> Result<Vec<MatchWithTeams>> {
    let (start_ms, end_ms) = get_ms_range(day)?;
    let matches = repo.get_all_matches_for_day(start_ms, end_ms)?;
    let mut filled_matches = vec![];
    for match_row in matches {
        if let Some(m) = with_teams_and_players(&repo, &main_character, match_row)? {
            filled_matches.push(m);
        }
    }
    Ok(filled_matches)
}

fn create_sessions(matches: Vec<MatchWithTeams>) -> Vec<DaySessionDto> {
    let mut sessions = vec![];

    for m in matches {
        if let Some(session) = sessions.last_mut() {
            if belongs_to_session(session, &m) {
                add_match_to_session(session, &m);
                continue;
            }
        }

        sessions.push(create_session(&m));
    }

    sessions
}

fn belongs_to_session(session: &DaySessionDto, m: &MatchWithTeams) -> bool {
    session.ended_at.saturating_add(SESSION_PAUSE_MS) >= m.match_row.created_at
        && session.playlist == m.match_row.playlist
}

fn add_match_to_session(session: &mut DaySessionDto, m: &MatchWithTeams) {
    if let SessionTypeDto::Team(team) = &session.session_type {
        let session_team: HashSet<&PlayerDto> = team.iter().collect();
        let players = to_team_session_type_dto(&m.own_team.players);
        let match_team: HashSet<&PlayerDto> = players.iter().collect();

        if session_team != match_team {
            session.session_type = SessionTypeDto::Solo;
        }
    }

    session.matches.push(DayMatchDto {
        match_guid: m.match_row.match_guid,
        won: m.won,
        own_score: m.own_team.score,
        enemy_score: m.enemy_team.score,
    });
    session.ended_at = m.match_row.ended_at;
}

fn create_session(m: &MatchWithTeams) -> DaySessionDto {
    let matches = vec![DayMatchDto {
        match_guid: m.match_row.match_guid,
        won: m.won,
        own_score: m.own_team.score,
        enemy_score: m.enemy_team.score,
    }];

    DaySessionDto {
        playlist: m.match_row.playlist,
        created_at: m.match_row.created_at,
        ended_at: m.match_row.ended_at,
        session_type: SessionTypeDto::Team(to_team_session_type_dto(&m.own_team.players)),
        matches,
    }
}

fn to_team_session_type_dto(players: &Vec<Player>) -> Vec<PlayerDto> {
    players
        .iter()
        .map(|p| PlayerDto {
            primary_id: p.primary_id.clone(),
            display_name: p.display_name.clone(),
        })
        .collect()
}

fn with_teams_and_players(
    repo: &Repository,
    main_character: &crate::core::bl::query_models::GlobalPlayerRow,
    match_row: DayMatchRow,
) -> Result<Option<MatchWithTeams>> {
    let teams = get_teams_by_match_id(repo, match_row.id)?;

    let mut own_team = None;
    let mut enemy_team = None;

    for team_row in teams {
        let players = get_players_by_team_id(repo, team_row.id)?;

        let is_own_team = is_main_character_team(
            main_character,
            players
                .iter()
                .map(|player| player.global_player_id)
                .collect(),
        );

        let team = Team {
            id: team_row.id,
            score: team_row.score,
            players: players
                .into_iter()
                .map(|player| Player {
                    primary_id: player.primary_id,
                    display_name: player.display_name,
                })
                .collect(),
        };

        if is_own_team {
            own_team = Some(team);
        } else {
            enemy_team = Some(team);
        }
    }

    let (Some(own_team), Some(enemy_team)) = (own_team, enemy_team) else {
        return Ok(None);
    };

    if own_team.score == enemy_team.score || own_team.players.len() != enemy_team.players.len() {
        return Ok(None);
    }

    Ok(Some(MatchWithTeams {
        match_row,
        won: own_team.score > enemy_team.score,
        own_team,
        enemy_team,
    }))
}

fn get_ms_range(day: Date) -> Result<(i64, i64)> {
    let tz = TimeZone::system();
    let zoned_day = day
        .to_zoned(tz)
        .map_err(|err| Error::CalculationError(err.to_string()))?;
    let start_ms = zoned_day
        .saturating_add(4.hours())
        .timestamp()
        .as_millisecond();

    let end_ms = zoned_day
        .saturating_add(1.days().hours(4))
        .timestamp()
        .as_millisecond();

    Ok((start_ms, end_ms))
}

#[cfg(test)]
mod tests {
    use jiff::{civil::date, tz::TimeZone};

    use crate::core::bl::features::day::get_ms_range;

    #[test]
    fn get_ms_range_test() {
        let tz = TimeZone::system();
        let day = date(2026, 8, 29);
        let range = get_ms_range(day);
        match range {
            Ok((start, end)) => {
                let dt = date(2026, 8, 29)
                    .at(4, 0, 0, 0)
                    .to_zoned(tz.clone())
                    .expect("Cannot convert to zoned");
                assert_eq!(start, dt.timestamp().as_millisecond());

                let dt = date(2026, 8, 30)
                    .at(4, 0, 0, 0)
                    .to_zoned(tz)
                    .expect("Cannot convert to zoned");
                assert_eq!(end, dt.timestamp().as_millisecond());
            }
            Err(_) => panic!("get_ms_range should not fail"),
        }
    }
}
