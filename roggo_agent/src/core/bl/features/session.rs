use std::collections::HashSet;
use std::path::Path;

use crate::core::api::contract::{DayDto, DayMatchDto, DaySessionDto, PlayerDto, SessionTypeDto};
use crate::core::bl::features::{get_most_played_player, is_main_character_team};
use crate::core::bl::{Error, Result};
use crate::core::db::Repository;
use crate::core::db::repository_features::day::get_teams_by_match_id;
use crate::core::db::repository_features::day::{DayMatchRow, get_players_by_team_id};
use jiff::ToSpan;
use jiff::civil::Date;
use jiff::tz::TimeZone;

pub fn get(path: &Path) -> Result<DayDto> {
    let repo = Repository::connect(path)?;

    let main_character = get_most_played_player(&repo)?;

    let filled_matches = get_full_matches(day, repo, main_character)?;

    let sessions = create_sessions(filled_matches);

    Ok(DayDto { sessions })
}