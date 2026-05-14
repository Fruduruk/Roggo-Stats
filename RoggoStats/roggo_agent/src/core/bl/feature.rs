use std::path::Path;

use crate::core::api::dto::{MainCharacterDto, PersonalMatchDto};
use crate::core::bl::{Error, Result};
use crate::core::db::Repository;

pub fn get_all_matches(path: &Path) -> Result<Vec<PersonalMatchDto>> {
    let repo = Repository::connect(path)?;

    let global_player = repo
        .get_player_with_most_replays()
        .map_err(|err| Error::NoPlayerFound { source: err })?;

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
            let mut own_team = false;

            for player in &players {
                if player.global_player_id == global_player.id {
                    own_team = true;
                }
            }

            if own_team {
                own_team_score = team.score;
                own_player_count = players.len() as i64;
            } else {
                enemy_team_score = team.score;
                enemy_player_count = players.len() as i64;
            }
        }

        if own_player_count != enemy_player_count {
            continue;
        }

        dtos.push(PersonalMatchDto {
            match_guid: match_row.match_guid,
            duration: match_row.duration,
            ended_at: match_row.ended_at,
            own_team_score,
            enemy_team_score,
            own_player_count,
            enemy_player_count,
        })
    }

    Ok(dtos)
}

pub fn get_main_character(path: &Path) -> Result<MainCharacterDto> {
    let repo = Repository::connect(path)?;
    let global_player = repo
        .get_player_with_most_replays()
        .map_err(|err| Error::NoPlayerFound { source: err })?;

    Ok(MainCharacterDto {
        username: global_player.last_username,
        primary_id: global_player.primary_id,
    })
}
