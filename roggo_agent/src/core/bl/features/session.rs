use std::path::Path;

use crate::core::api::contract::session::DetailedSessionDto;
use crate::core::bl::features::{get_most_played_player};
use crate::core::bl::{Error, Result};
use crate::core::db::Repository;
use uuid::Uuid;

pub fn get(path: &Path, match_guids: Vec<Uuid>) -> Result<DetailedSessionDto> {
    let repo = Repository::connect(path)?;

    let main_character = get_most_played_player(&repo)?;

    todo!()
    // Ok(DetailedSessionDto {
    //     session_matches: (),
    //     own_team_player_averages: (),
    //     average_enemy_core_stats: (),
    //     average_team_player_core_stats: (),
    //     average_team_player_advanced_stats: (),
    // })
}
