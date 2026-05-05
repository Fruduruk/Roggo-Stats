use uuid::Uuid;

use crate::core::models::{
    api_models::{BallHit, Event, Player, UpdateState},
    game_stats::{GameStats, PlayerStats, TeamStats, TimeState},
};

#[derive(Debug)]
struct GameState {
    in_replay: bool,
    finished: bool,
    in_overtime: bool,
    timestamp: Option<u128>,
    last_state_update_timestamp: Option<u128>,
    state_update_timestamp: Option<u128>,
}

impl Default for GameState {
    fn default() -> GameState {
        Self {
            in_replay: false,
            finished: false,
            in_overtime: false,
            timestamp: None,
            last_state_update_timestamp: None,
            state_update_timestamp: None,
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

    pub fn insert(&mut self, timestamp: u128, event: Event) {
        self.state.timestamp = Some(timestamp);

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

    fn insert_ball_hit(&mut self, _ball_hit: BallHit) {
        if !self.state.in_replay {}
    }

    fn insert_update_state(&mut self, update_state: UpdateState) {
        self.update_game_state(&update_state);
        self.update_game_stats(update_state);
    }

    #[inline]
    fn state_update_time_delta(&self) -> Option<u128> {
        Some(self.state.state_update_timestamp? - self.state.last_state_update_timestamp?)
    }

    fn update_game_state(&mut self, update_state: &UpdateState) {
        if self.state.state_update_timestamp != self.state.timestamp {
            self.state.last_state_update_timestamp = self.state.state_update_timestamp;
        }
        self.state.state_update_timestamp = self.state.timestamp;

        self.state.in_replay = update_state.game.b_replay;
        self.state.in_overtime = update_state.game.b_overtime;
    }

    fn update_game_stats(&mut self, update_state: UpdateState) {
        if update_state.game.b_replay {
            return;
        }
        if self.time_state_update_reasonable(&update_state) {
            self.update_time_state(&update_state);
        }

        self.stats.arena_name.get_or_insert(update_state.game.arena);

        if self.stats.winner.is_none() {
            self.stats.winner = update_state
                .game
                .b_has_winner
                .then_some(update_state.game.winner);
        }

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

        for player in update_state.players {
            let delta = self.state_update_time_delta();

            let team = self
                .stats
                .teams
                .get_mut(&player.team_num)
                .unwrap_or_else(|| panic!("Team doesn't exist for player {}", player.name.clone()));

            let player_stats =
                team.players
                    .entry(player.primary_id.clone())
                    .or_insert(PlayerStats::new(
                        player.name.clone(),
                        player.primary_id.clone(),
                    ));

            player_stats.score = player.score;
            player_stats.goals = player.goals;
            player_stats.shots = player.shots;
            player_stats.assists = player.assists;
            player_stats.saves = player.saves;
            player_stats.touches = player.touches;
            player_stats.car_touches = player.car_touches;
            player_stats.demos = player.demos;
            if let Some(delta) = delta {
                increment_counters(player_stats, &player, delta);
            }
        }
    }

    fn time_state_update_reasonable(&self, _update_state: &UpdateState) -> bool {
        true
    }

    fn update_time_state(&mut self, update_state: &UpdateState) {
        // self.stats.states.push(TimeState {
        //     timestamp: self.state.current_timestamp,
        //     ball_speed: update_state.game.ball_state.speed,
        // });
    }
}

fn increment_counters(player_stats: &mut PlayerStats, player: &Player, time_delta: u128) {
    if player.b_boosting == Some(true) {
        *player_stats.time_boosting.get_or_insert(0) += time_delta;
    }
    if player.b_demolished == Some(true) {
        *player_stats.time_demolished.get_or_insert(0) += time_delta;
    }
    if player.b_on_ground == Some(true) {
        *player_stats.time_on_ground.get_or_insert(0) += time_delta;
    }
    if player.b_on_wall == Some(true) {
        *player_stats.time_on_wall.get_or_insert(0) += time_delta;
    }
    if player.b_powersliding == Some(true) {
        *player_stats.time_powersliding.get_or_insert(0) += time_delta;
    }
    if player.b_supersonic == Some(true) {
        *player_stats.time_supersonic.get_or_insert(0) += time_delta;
    }
}
