use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct MatchDto {
    pub match_guid: Uuid,
    pub arena: String,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub had_overtime: bool,
    pub deleted: bool,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct MainCharacterDto {
    pub username: String,
    pub primary_id: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub enum AgentErrorCode {
    NoEntries,
    InternalError,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct AgentErrorDto {
    pub error: AgentErrorCode,
    pub message: String,
    pub details: Vec<String>,
}
