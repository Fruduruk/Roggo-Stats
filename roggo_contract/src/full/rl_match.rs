use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::{Playlist, full::Team};

#[derive(Debug, Serialize, Deserialize)]
pub struct RLMatch {
    pub match_guid: Uuid,
    pub arena: String,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub had_overtime: bool,
    pub deleted: bool,
    pub playlist: Playlist,
    pub teams: Vec<Team>,
}
