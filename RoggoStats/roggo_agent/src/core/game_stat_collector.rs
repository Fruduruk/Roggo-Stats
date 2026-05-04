use uuid::Uuid;

use crate::core::models::{api_models::Event, game_stats::GameStats};

pub struct GameStatCollector {
    uuid: Option<Uuid>,
    pub finished: bool,
    pub game_stats: GameStats,
}

impl Default for GameStatCollector {
    fn default() -> Self {
        Self {
            uuid: None,
            finished: false,
            game_stats: GameStats::default(),
        }
    }
}

impl GameStatCollector {
    pub fn insert(&self, event: Event) {
        match event {
            Event::UpdateState(update_state) => todo!(),
            Event::BallHit(ball_hit) => todo!(),
            Event::ClockUpdatedSeconds(clock_updated_seconds) => todo!(),
            Event::CountdownBegin(match_identfier) => todo!(),
            Event::CrossbarHit(crossbar_hit) => todo!(),
            Event::GoalReplayEnd(match_identfier) => todo!(),
            Event::GoalReplayStart(match_identfier) => todo!(),
            Event::GoalReplayWillEnd(match_identfier) => todo!(),
            Event::ReplayWillEnd(match_identfier) => todo!(),
            Event::GoalScored(goal_scored) => todo!(),
            Event::MatchCreated(match_identfier) => todo!(),
            Event::MatchInitialized(match_identfier) => todo!(),
            Event::MatchDestroyed(match_identfier) => todo!(),
            Event::MatchEnded(match_ended) => todo!(),
            Event::MatchPaused(match_identfier) => todo!(),
            Event::MatchUnpaused(match_identfier) => todo!(),
            Event::PodiumStart(match_identfier) => todo!(),
            Event::RoundStarted(match_identfier) => todo!(),
            Event::StatfeedEvent(statfeed_event) => todo!(),
            _ => return,
        }
    }

    pub fn export(self) -> GameStats {
        self.game_stats
    }
}
