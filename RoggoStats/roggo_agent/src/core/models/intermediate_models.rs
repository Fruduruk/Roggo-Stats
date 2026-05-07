use std::collections::HashMap;

use uuid::Uuid;

use crate::core::models::api_models::{Ball, Location};

#[derive(Debug)]
pub struct GameStats {
    pub match_guid: Uuid,
    pub arena_name: Option<String>,
    pub duration: i64,
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
            duration: 0,
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
pub struct Goal {}

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

    pub advanced_stats: Option<AdvancedPlayerStats>,

    pub ball_hits: Vec<Ball>,
    pub crossbar_hits: Vec<CrossbarHitStatistic>,
    pub goal_stats: Vec<Goal>,
}

#[derive(Debug)]
pub struct AdvancedPlayerStats {
    pub time_boosting: i64,
    pub time_demolished: i64,
    pub time_on_ground: i64,
    pub time_on_wall: i64,
    pub time_powersliding: i64,
    pub time_supersonic: i64,
}

impl Default for AdvancedPlayerStats {
    fn default() -> Self {
        Self {
            time_boosting: Default::default(),
            time_demolished: Default::default(),
            time_on_ground: Default::default(),
            time_on_wall: Default::default(),
            time_powersliding: Default::default(),
            time_supersonic: Default::default(),
        }
    }
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
            advanced_stats: None,
            ball_hits: vec![],
            crossbar_hits: vec![],
            goal_stats: vec![],
        }
    }
}
