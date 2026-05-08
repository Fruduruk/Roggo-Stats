use std::collections::HashMap;

use uuid::Uuid;

use crate::core::models::api_models::{Ball, BallState, Location};

#[derive(Debug)]
pub struct GameStats {
    pub match_guid: Uuid,
    pub arena_name: Option<String>,
    pub duration: i64,
    pub created_at_timestamp: i64,
    pub ended_at_timestamp: i64,
    pub had_overtime: bool,
    pub teams: HashMap<u8, TeamStats>,
    pub clock_samples: Vec<ClockSample>,
    pub crossbar_hits: Vec<CrossbarHitStatistic>,
    pub goal_details: Vec<GoalDetails>,
    pub ball_hits: Vec<BallHitStatistic>,
    pub statfeed_events: Vec<StatfeedEventStatistic>,

    pub timeline: Vec<TimelineInstant>,

    pub excluded_timeline_instants: u128,
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
            teams: HashMap::new(),
            clock_samples: vec![],
            crossbar_hits: vec![],
            goal_details: vec![],
            ball_hits: vec![],
            statfeed_events: vec![],

            timeline: vec![],
            excluded_timeline_instants: 0,
        }
    }
}

#[derive(Debug)]
pub struct ClockSample {
    pub timestamp: i64,
    pub time_seconds: u16,
    pub is_overtime: bool,
}

#[derive(Debug)]
pub struct TimelineInstant {
    pub timestamp: i64,
    pub ball_state: BallState,
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
    pub timestamp: i64,
    pub ball_speed: f64,
    pub impact_force: f64,
    pub location: Location,
    pub last_touch_speed: f64,
}

#[derive(Debug)]
pub struct BallHitStatistic {
    pub timestamp: i64,
    pub pre_hit_speed: f64,
    pub post_hit_speed: f64,
    pub location: Location,
    pub player_primary_id: String,
}

#[derive(Debug)]
pub struct GoalDetails {
    pub timestamp: i64,
    pub goal_time: f64,
    pub impact_location: Location,
    pub goal_speed: f64,
    pub last_touch_speed: f64,
    pub scorer_primary_id: String,
    pub assister_primary_id: Option<String>,
    pub last_touch_primary_id: String,
}
#[derive(Debug)]
pub struct StatfeedEventStatistic {
    pub timestamp: i64,
    pub event_name: String,
    pub event_type: String,
    pub main_target_primary_id: String,
    pub secondary_target_primary_id: Option<String>,
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

    pub advanced_stats: Option<AdvancedPlayerStats>,

    pub crossbar_hits: Vec<CrossbarHitStatistic>,
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
            crossbar_hits: vec![],
        }
    }
}
