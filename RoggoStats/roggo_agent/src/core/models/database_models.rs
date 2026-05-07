use std::collections::HashMap;

use uuid::Uuid;

use crate::core::models::api_models::{Ball, Location};

#[derive(Debug)]
pub struct GameStats {
    pub match_guid: Uuid,
    pub arena_name: Option<String>,
    pub created_at_timestamp: i64,
    pub ended_at_timestamp: i64,
    pub had_overtime: bool,
    pub winner: Option<String>,
    pub states: Vec<TimeState>,
    pub teams: HashMap<u8, TeamStats>,
}

impl GameStats {
    pub fn new(match_guid: Uuid, timestamp: i64) -> Self {
        Self {
            match_guid,
            arena_name: None,
            created_at_timestamp: timestamp,
            ended_at_timestamp: timestamp,
            had_overtime: false,
            winner: None,
            states: vec![],
            teams: HashMap::new(),
        }
    }
}

#[derive(Debug)]
pub struct TimeState {
    pub timestamp: i64,
    pub ball_speed: f64,
}

#[derive(Debug)]
pub struct TeamStats {
    pub name: String,
    pub score: u16,
    pub color_primary: String,
    pub color_secondary: String,
    pub players: HashMap<String, PlayerStats>,
}

impl TeamStats {
    pub fn new(name: String, color_primary: String, color_secondary: String) -> Self {
        Self {
            name,
            score: 0,
            color_primary,
            color_secondary,
            players: HashMap::new(),
        }
    }
}
#[derive(Debug)]
pub struct CrossbarHitStatistic {
    pub hit_speed: f64,
    pub last_touch_speed: f64,
    pub location: Location,
    pub impact_force: f64,
}

#[derive(Debug)]
pub struct Goal {

}

#[derive(Debug)]
pub struct PlayerStats {
    pub name: String,
    pub primary_id: String,
    pub shortcut: u8,
    pub score: u16,
    pub goals: u16,
    pub shots: u16,
    pub assists: u16,
    pub saves: u16,
    pub touches: u32,
    pub car_touches: u32,
    pub demos: u16,

    pub time_boosting: Option<i64>,
    pub time_demolished: Option<i64>,
    pub time_on_ground: Option<i64>,
    pub time_on_wall: Option<i64>,
    pub time_powersliding: Option<i64>,
    pub time_supersonic: Option<i64>,

    pub ball_hits: Vec<Ball>,
    pub crossbar_hits: Vec<CrossbarHitStatistic>,
    pub goal_stats: Vec<Goal>,
}

impl PlayerStats {
    pub fn new(name: String, primary_id: String) -> Self {
        Self {
            name,
            primary_id,
            shortcut: 0,
            score: 0,
            goals: 0,
            shots: 0,
            assists: 0,
            saves: 0,
            touches: 0,
            car_touches: 0,
            demos: 0,
            time_boosting: None,
            time_demolished: None,
            time_on_ground: None,
            time_on_wall: None,
            time_powersliding: None,
            time_supersonic: None,
            ball_hits: vec![],
            crossbar_hits: vec![],
            goal_stats: vec![],
        }
    }
}
