use uuid::Uuid;

use crate::core::models::{
    api_models::{BallHit, CrossbarHit, Event, GamePlayer, GoalScored, Player, Team, UpdateState},
    game_stats::{CrossbarHitStatistic, GameStats, Goal, PlayerStats, TeamStats, TimeState},
};

#[derive(Debug)]
struct GameState {
    in_replay: bool,
    finished: bool,
    in_overtime: bool,
    timestamp: Option<u128>,
    last_state_update_timestamp: Option<u128>,
    state_update_timestamp: Option<u128>,
    count_down: bool,
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
            count_down: false,
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
        // println!("{:#?}", self.state);
        match event {
            Event::UpdateState(update_state) => self.insert_update_state(update_state),
            // Event::BallHit(ball_hit) => self.insert_ball_hit(ball_hit),
            // Event::ClockUpdatedSeconds(clock_updated_seconds) => todo!(),
            Event::CountdownBegin(_) => {
                self.state.last_state_update_timestamp = None;
                self.state.count_down = true;
            }
            Event::CrossbarHit(crossbar_hit) => self.insert_crossbar_hit(crossbar_hit),
            Event::GoalScored(goal_scored) => self.insert_goal_scored(goal_scored),
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
        if self.state.in_replay {
            return;
        }

        for player in ball_hit.players {
            if let Some(player_stats) = self.get_player_stats(&player) {
                player_stats.ball_hits.push(ball_hit.ball);
            }
        }
    }

    fn get_player_stats(&mut self, player: &GamePlayer) -> Option<&mut PlayerStats> {
        self.stats
            .teams
            .get_mut(&player.team_num)?
            .players
            .get_mut(&player.name)
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
        self.update_timestamps();

        self.state.in_replay = update_state.game.b_replay;
        self.state.in_overtime = update_state.game.b_overtime;

        self.update_countdown(update_state);
    }

    fn update_timestamps(&mut self) {
        if self.state.state_update_timestamp != self.state.timestamp {
            self.state.last_state_update_timestamp = self.state.state_update_timestamp;
        }
        self.state.state_update_timestamp = self.state.timestamp;
    }

    fn update_countdown(&mut self, update_state: &UpdateState) {
        let anyone_moving_while_countdown_is_set = self.state.count_down
            && update_state
                .players
                .iter()
                .any(|player| player.speed.is_some_and(|speed| speed > 0f64));

        if anyone_moving_while_countdown_is_set {
            self.state.count_down = false;
        }
    }

    fn update_game_stats(&mut self, update_state: UpdateState) {
        if update_state.game.b_replay {
            return;
        }
        if self.state.count_down {
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
            let score = team.score;
            self.get_or_create_team_stats_mut(team).score = score;
        }

        for player in update_state.players {
            let delta = self.state_update_time_delta();

            let player_stats = self.get_or_create_player_stats_mut(player.clone());

            update_core_player_stats(&player, player_stats);
            if let Some(delta) = delta {
                increment_counters(player_stats, &player, delta);
            }
        }
    }

    fn get_or_create_team_stats_mut(&mut self, team: Team) -> &mut TeamStats {
        self.stats
            .teams
            .entry(team.team_num)
            .or_insert(TeamStats::new(
                team.name,
                team.color_primary,
                team.color_secondary,
            ))
    }

    fn get_or_create_player_stats_mut(&mut self, player: Player) -> &mut PlayerStats {
        self.stats
            .teams
            .get_mut(&player.team_num)
            .unwrap_or_else(|| panic!("Team doesn't exist for player {}", &player.name))
            .players
            .entry(player.name.clone())
            .or_insert(PlayerStats::new(player.name, player.primary_id))
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

    fn insert_crossbar_hit(&mut self, crossbar_hit: CrossbarHit) {
        if let Some(player_stats) = self.get_player_stats(&crossbar_hit.ball_last_touch.player) {
            player_stats.crossbar_hits.push(CrossbarHitStatistic {
                hit_speed : crossbar_hit.ball_speed,
                last_touch_speed: crossbar_hit.ball_last_touch.speed,
                location: crossbar_hit.ball_location,
                impact_force: crossbar_hit.impact_force
            });
        }
    }
    
    fn insert_goal_scored(&mut self, goal_scored: GoalScored) {
        if let Some(player_stats) = self.get_player_stats(&goal_scored.scorer){
            player_stats.goal_stats.push(
                Goal{}
            );
        }
    }
}

fn update_core_player_stats(player: &Player, player_stats: &mut PlayerStats) {
    player_stats.score = player.score;
    player_stats.goals = player.goals;
    player_stats.shots = player.shots;
    player_stats.assists = player.assists;
    player_stats.saves = player.saves;
    player_stats.touches = player.touches;
    player_stats.car_touches = player.car_touches;
    player_stats.demos = player.demos;
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
