use serde::{Deserialize, Serialize};
use uuid::Uuid;

pub struct MatchSummary{
    
}

#[derive(Debug, Serialize, Deserialize)]
pub struct MatchDto {
    pub match_guid: Uuid,
    pub arena: String,
    pub duration_seconds: i64,
}