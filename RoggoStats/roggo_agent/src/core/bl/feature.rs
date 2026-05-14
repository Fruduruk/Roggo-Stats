use std::path::Path;

use crate::core::api::dto::{MainCharacterDto, MatchDto};
use crate::core::bl::{Error, Result};
use crate::core::db::Repository;

pub fn get_all_matches(path: &Path) -> Result<Vec<MatchDto>> {
    let repo = Repository::connect(path)?;
    let matches = repo.get_match()?;

    Ok(matches
        .into_iter()
        .map(|match_row| MatchDto::from(match_row))
        .collect())
}

pub fn get_main_character(path: &Path) -> Result<MainCharacterDto> {
    let repo = Repository::connect(path)?;
    let play_counts = repo.get_player_play_count()?;

    let max = play_counts
        .into_iter()
        .max_by_key(|row| row.play_count)
        .ok_or_else(|| Error::NoPlayerFound)?;

    Ok(MainCharacterDto {
        username: max.last_username,
        primary_id: max.primary_id,
    })
}
