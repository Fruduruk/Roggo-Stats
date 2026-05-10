use std::path::Path;

use crate::core::api::dto::MatchDto;
use crate::core::db::repository::{self, Repository};
use crate::core::bl::{Result,Error};



pub fn get_all_matches(db_path: &Path) -> Result<Vec<MatchDto>>{
    let repo = Repository::connect(db_path)?;
    let matches = repo.get_match()?;

    Ok(matches.into_iter().map(|match_row| MatchDto::from(match_row)).collect())
}