use uuid::Uuid;

use crate::core::models::{
    api_models::{BallHit, Event, UpdateState},
    game_stats::{GameStats, TeamStats, TimeState},
};

struct GameState {
    in_replay: bool,
    finished: bool,
    in_overtime: bool,
}

impl Default for GameState {
    fn default() -> GameState {
        Self {
            in_replay: false,
            finished: false,
            in_overtime: false,
        }
    }
}

pub struct GameStatCollector {
    stats: GameStats,
    state: GameState,
}

impl GameStatCollector {
    pub fn new(match_guid: Uuid) -> Self {
        Self {
            stats: GameStats::new(match_guid),
            state: GameState::default(),
        }
    }

    pub fn get_match_guid(&self) -> Uuid {
        self.stats.match_guid
    }

    pub fn export(self) -> GameStats {
        self.stats
    }

    pub fn is_finished(&self) -> bool {
        self.state.finished
    }

    pub fn insert(&mut self, event: Event) {
        match event {
            Event::UpdateState(update_state) => self.insert_update_state(update_state),
            Event::BallHit(ball_hit) => self.insert_ball_hit(ball_hit),
            // Event::ClockUpdatedSeconds(clock_updated_seconds) => todo!(),
            // Event::CountdownBegin(_) => todo!(),
            // Event::CrossbarHit(crossbar_hit) => todo!(),
            // Event::GoalScored(goal_scored) => todo!(),
            // Event::MatchCreated(_) => todo!(),
            // Event::MatchInitialized(_) => todo!(),
            Event::MatchDestroyed(_) => {
                println!("Game finished, because match is destroyed");
                self.state.finished = true;
            }
            Event::MatchEnded(_match_ended) => {
                println!("Game finished, because match ended");
                self.state.finished = true;
            }
            Event::PodiumStart(_) => {
                println!("Game finished, because podium started");
                self.state.finished = true;
            }
            // Event::RoundStarted(_) => todo!(),
            // Event::StatfeedEvent(statfeed_event) => todo!(),
            _ => return,
        }
    }

    fn insert_ball_hit(&mut self, ball_hit: BallHit) {
        if !self.state.in_replay {}
    }

    fn insert_update_state(&mut self, update_state: UpdateState) {
        self.update_game_state(&update_state);
        self.update_game_stats(update_state);
    }

    fn update_game_state(&mut self, update_state: &UpdateState) {
        self.state.in_replay = update_state.game.b_replay;
        self.state.in_overtime = update_state.game.b_overtime;
    }

    fn update_game_stats(&mut self, update_state: UpdateState) {
        if update_state.game.b_replay {
            return;
        }

        self.stats.arena_name.get_or_insert(update_state.game.arena);
        self.stats.winner = update_state
            .game
            .b_has_winner
            .then_some(update_state.game.winner);

        self.stats.states.push(TimeState {
            seconds_left: update_state.game.time_seconds,
            ball_speed: update_state.game.ball_state.speed,
        });

        for team in update_state.game.teams {
            let team_stats = self
                .stats
                .teams
                .entry(team.team_num)
                .or_insert(TeamStats::new(
                    team.name,
                    team.color_primary,
                    team.color_secondary,
                ));

            team_stats.score = team.score;
        }
    }
}
