use uuid::Uuid;

use crate::core::models::{api_models::{BallHit, Event}, game_stats::GameStats};

pub struct GameStatCollector {
    match_guid: Uuid,
    finished: bool,
    game_stats: GameStats,
}

impl GameStatCollector {
    pub fn new(match_guid: Uuid) -> Self {
        Self {
            match_guid,
            finished: false,
            game_stats: GameStats::default(),
        }
    }

    pub fn get_match_guid(&self) -> Uuid {
        self.match_guid
    }

    pub fn export(self) -> GameStats {
        self.game_stats
    }

    pub fn is_finished(&self) -> bool {
        self.finished
    }

    pub fn insert(&mut self, event: Event) {
        match event {
            // Event::UpdateState(update_state) => todo!(),
            Event::BallHit(ball_hit) => self.insert_ball_hit(ball_hit),
            // Event::ClockUpdatedSeconds(clock_updated_seconds) => todo!(),
            // Event::CountdownBegin(_) => todo!(),
            // Event::CrossbarHit(crossbar_hit) => todo!(),
            // Event::GoalScored(goal_scored) => todo!(),
            // Event::MatchCreated(_) => todo!(),
            // Event::MatchInitialized(_) => todo!(),
            Event::MatchDestroyed(_) => self.finished = true,
            Event::MatchEnded(_match_ended) => self.finished = true,
            Event::PodiumStart(_) => self.finished = true,
            // Event::RoundStarted(_) => todo!(),
            // Event::StatfeedEvent(statfeed_event) => todo!(),
            _ => return,
        }
    }
    
    fn insert_ball_hit(&mut self, ball_hit: BallHit) {
        self.game_stats.ball_hits.push(ball_hit);
    }
}
