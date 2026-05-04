use std::{collections::HashMap};

use uuid::Uuid;

#[derive(Debug)]
pub struct GameStats {
    pub match_guid: Uuid,
    pub arena_name: Option<String>,
    pub winner: Option<String>,
    pub states: Vec<TimeState>,
    pub teams: HashMap<u8, TeamStats>,
}

impl GameStats {
    pub fn new(match_guid: Uuid) -> Self {
        Self {
            match_guid,
            arena_name: None,
            winner: None,
            states: vec![],
            teams: HashMap::new(),
        }
    }
}

#[derive(Debug)]
pub struct TimeState {
    pub ball_speed: f64,
    pub seconds_left: i32,
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
pub struct PlayerStats {
    pub name: String,
    pub primary_id: String,
    pub score: u16,
    pub goals: u16,
    pub shots: u16,
    pub assists: u16,
    pub saves: u16,
    pub touches: u32,
    pub car_touches: u32,
    pub demos: u16,
}

impl PlayerStats {
    pub fn new(name: String, primary_id: String) -> Self {
        Self {
            name,
            primary_id,
            score: 0,
            goals: 0,
            shots: 0,
            assists: 0,
            saves: 0,
            touches: 0,
            car_touches: 0,
            demos: 0,
        }
    }
}
