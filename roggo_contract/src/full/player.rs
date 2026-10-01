use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize)]
pub struct Player {
    pub primary_id: String,
    pub last_username: String,
    pub display_name: String,
    pub shortcut: i64,
    pub score: i64,
    pub goals: i64,
    pub shots: i64,
    pub assists: i64,
    pub saves: i64,
    pub touches: i64,
    pub car_touches: i64,
    pub demos: i64,
    pub player_stats: Option<PlayerStats>,
    pub statfeed_events: Vec<StatfeedEvent>
}

#[derive(Debug, Serialize, Deserialize)]
pub struct PlayerStats {
    pub percent_boosting: f64,
    pub percent_demolished: f64,
    pub percent_on_ground: f64,
    pub percent_on_wall: f64,
    pub percent_powersliding: f64,
    pub percent_supersonic: f64,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct StatfeedEvent {
    pub timestamp: f32,
    pub event_name: String,
    pub event_type: String,
    pub secondary_target_primary_id: Option<String>,
}