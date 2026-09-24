use serde::{Deserialize, Serialize};
use uuid::Uuid;



#[derive(Debug, Serialize, Deserialize)]
pub struct DayDto {
    pub sessions: Vec<DaySessionDto>,
}

#[derive(Debug, Serialize, Deserialize, Hash)]
pub enum SessionTypeDto {
    Solo,
    Team(Vec<super::PlayerDto>),
}

#[derive(Debug, Serialize, Deserialize, Hash)]
pub struct DaySessionDto {
    pub playlist: super::Playlist,
    pub created_at: i64,
    pub ended_at: i64,
    pub session_type: SessionTypeDto,
    pub matches: Vec<DayMatchDto>,
}

#[derive(Debug, Serialize, Deserialize, Hash)]
pub struct DayMatchDto {
    pub match_guid: Uuid,
    pub won: bool,
    pub own_score: i64,
    pub enemy_score: i64,
    pub created_at: i64,
    pub ended_at: i64,
}

// ------------------

#[derive(Debug, Serialize, Deserialize)]
pub struct DaysPlayedDto {
    pub days: Vec<String>,
}