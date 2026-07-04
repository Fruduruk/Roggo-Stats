use std::collections::HashMap;

use uuid::Uuid;

use crate::core::rl_api::{
    self,
    models::{ Location},
};

#[derive(Debug)]
pub struct GameState {
    pub round_started_once: bool,
    pub match_started: bool,
    pub in_replay: bool,
    pub finished: bool,
    pub in_overtime: bool,
    pub timestamp: Option<i64>,
    pub last_state_update_timestamp: Option<i64>,
    pub state_update_timestamp: Option<i64>,
    pub count_down: bool,
    pub goal_scored: bool,
}

impl GameState {
    #[inline]
    pub fn state_update_time_delta(&self) -> Option<i64> {
        Some(self.state_update_timestamp? - self.last_state_update_timestamp?)
    }
}

impl Default for GameState {
    fn default() -> GameState {
        Self {
            round_started_once: false,
            match_started: false,
            in_replay: false,
            finished: false,
            in_overtime: false,
            timestamp: None,
            last_state_update_timestamp: None,
            state_update_timestamp: None,
            count_down: false,
            goal_scored: false,
        }
    }
}

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
        }
    }

    #[inline]
    pub fn get_player_primary_id(&self, player: &rl_api::models::GamePlayer) -> Option<String> {
        Some(self.get_player_stats(&player)?.primary_id.clone())
    }

    #[inline]
    pub fn get_player_stats_mut(
        &mut self,
        player: &rl_api::models::GamePlayer,
    ) -> Option<&mut PlayerStats> {
        self.teams
            .get_mut(&player.team_num)?
            .players
            .get_mut(&player.name)
    }

    #[inline]
    pub fn get_player_stats(&self, player: &rl_api::models::GamePlayer) -> Option<&PlayerStats> {
        self.teams.get(&player.team_num)?.players.get(&player.name)
    }

    #[inline]
    pub fn get_or_create_team_stats_mut(&mut self, team: rl_api::models::Team) -> &mut TeamStats {
        self.teams.entry(team.team_num).or_insert(TeamStats::new(
            team.name,
            team.color_primary,
            team.color_secondary,
        ))
    }
}

#[derive(Debug)]
pub struct ClockSample {
    pub timestamp: i64,
    pub time_seconds: u16,
    pub is_overtime: bool,
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

#[derive(Debug, Default)]
pub struct AdvancedPlayerStats {
    pub time_boosting: i64,
    pub time_demolished: i64,
    pub time_on_ground: i64,
    pub time_on_wall: i64,
    pub time_powersliding: i64,
    pub time_supersonic: i64,
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

#[derive(Debug, Default)]
pub struct StatSnapshot {
    pub speed: f64,
    pub boost: f64,
}
