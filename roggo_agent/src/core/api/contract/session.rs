use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct DetailedSessionDto {
    pub session_matches: Vec<SessionMatchDto>,
    pub own_team_player_averages: Vec<DetailedAveragePlayerDto>,
    pub average_enemy_core_stats: Option<DetailedAverageCoreStatsDto>,
    pub average_team_player_core_stats: Option<DetailedAverageCoreStatsDto>,
    pub average_team_player_advanced_stats: Option<DetailedAverageAdvancedStatsDto>,
}



#[derive(Debug, Serialize, Deserialize)]
pub struct DetailedAverageAdvancedStatsDto {
    pub average_percent_boosting: f64,
    pub average_percent_demolished: f64,
    pub average_percent_on_ground: f64,
    pub average_percent_on_wall: f64,
    pub average_percent_powersliding: f64,
    pub average_percent_supersonic: f64,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct DetailedAverageCoreStatsDto {
    pub average_score: f64,
    pub average_goals: f64,
    pub average_shots: f64,
    pub average_shooting_percentage: Option<f64>,
    pub average_assists: f64,
    pub average_saves: f64,
    pub average_demos: f64,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct DetailedAveragePlayerDto {
    pub username: String,
    pub average_core_stats: DetailedAverageCoreStatsDto,
    pub average_advanced_stats: Option<DetailedAverageAdvancedStatsDto>,
}

#[derive(Debug, Serialize, Deserialize)]
pub enum MVPType {
    MVP,
    ACE,
    Nothing,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct SessionMatchDto {
    pub match_guid: Uuid,
    pub created_at: i64,
    pub ended_at: i64,
    pub won: Option<bool>,
    pub mvp_type: MVPType,
    pub hidden: bool,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct SessionRequest {
    pub match_guids: Vec<Uuid>,
}

// #[derive(Debug, Serialize, Deserialize)]
// pub struct SimpleSessionDto {
//     pub match_guids: Vec<Uuid>,
//     pub match_count: i64,
//     pub matches_won: i64,
//     pub created_at: i64,
//     pub ended_at: i64,
//     pub own_player_count: i64,
//     pub enemy_player_count: i64,
// }

