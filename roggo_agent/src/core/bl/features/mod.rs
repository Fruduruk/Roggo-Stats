pub mod day;
pub mod session;

use std::path::Path;

use crate::core::api::contract::{PlayerDto, VersionDto};
use crate::core::bl::query_models::{ GlobalPlayerRow};
use crate::core::bl::{Error, Result};
use crate::core::db::Repository;
use crate::AGENT_VERSION;

pub fn get_version() -> VersionDto {
    VersionDto {
        version: AGENT_VERSION.into(),
    }
}

pub fn get_main_character(path: &Path) -> Result<PlayerDto> {
    let repo = Repository::connect(path)?;
    let main_character = get_most_played_player(&repo)?;

    Ok(PlayerDto {
        display_name: main_character.last_username,
        primary_id: main_character.primary_id,
    })
}

pub fn get_most_played_player(repo: &Repository) -> Result<GlobalPlayerRow> {
    let global_player = repo
        .get_player_with_most_replays()
        .map_err(|err| Error::NoPlayerFound { source: err })?;
    Ok(global_player)
}

#[inline]
pub fn is_main_character_team(main_character: &GlobalPlayerRow, player_ids: Vec<i64>) -> bool {
    for id in player_ids {
        if id == main_character.id {
            return true;
        }
    }
    false
}