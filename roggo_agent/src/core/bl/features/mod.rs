pub mod day;


use uuid::Uuid;

use crate::AGENT_VERSION;
use crate::core::bl::query_models::{ GlobalPlayerRow};
use crate::core::bl::{Error, Result};
use crate::core::db::Repository;

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