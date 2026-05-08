use uuid::{Uuid, timestamp};

use crate::core::models::{
    api_models,
    intermediate_models::{
        BallHitStatistic, ClockSample, CrossbarHitStatistic, GameStats, GoalDetails, PlayerStats,
        StatfeedEventStatistic, TeamStats, TimelineInstant,
    },
};

#[derive(Debug)]
struct GameState {
    in_replay: bool,
    finished: bool,
    in_overtime: bool,
    timestamp: Option<i64>,
    last_state_update_timestamp: Option<i64>,
    state_update_timestamp: Option<i64>,
    count_down: bool,
    goal_scored: bool,
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
            goal_scored: false,
        }
    }
}

pub struct GameStatCollector {
    stats: GameStats,
    state: GameState,
}

impl GameStatCollector {
    pub fn new(match_guid: Uuid, timestamp: i64) -> Self {
        Self {
            stats: GameStats::new(match_guid, timestamp),
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

    pub fn insert(&mut self, timestamp: i64, event: api_models::Event) {
        self.state.timestamp = Some(timestamp);
        // println!("{:#?}", self.state);
        match event {
            api_models::Event::UpdateState(update_state) => self.insert_update_state(update_state),
            api_models::Event::BallHit(ball_hit) => self.insert_ball_hit(ball_hit),
            api_models::Event::ClockUpdatedSeconds(clock_updated_seconds) => {
                self.insert_clock_samples(clock_updated_seconds)
            }
            api_models::Event::CountdownBegin(_) => {
                self.state.count_down = true;
                self.state.goal_scored = false;
            }
            api_models::Event::CrossbarHit(crossbar_hit) => self.insert_crossbar_hit(crossbar_hit),
            api_models::Event::GoalScored(goal_scored) => self.insert_goal_scored(goal_scored),
            // Event::MatchCreated(_) => todo!(),
            // Event::MatchInitialized(_) => todo!(),
            api_models::Event::MatchDestroyed(_) => {
                println!("Game finished, because match is destroyed");
                self.stats.ended_at_timestamp = timestamp;
                self.state.finished = true;
            }
            api_models::Event::MatchEnded(_match_ended) => {
                println!("Game finished, because match ended");
                self.stats.ended_at_timestamp = timestamp;
                self.state.finished = true;
            }
            api_models::Event::PodiumStart(_) => {
                println!("Game finished, because podium started");
                self.stats.ended_at_timestamp = timestamp;
                self.state.finished = true;
            }
            // Event::RoundStarted(_) => todo!(),
            api_models::Event::StatfeedEvent(statfeed_event) => {
                self.insert_stat_feed_event(statfeed_event)
            }
            _ => return,
        }
    }

    fn insert_ball_hit(&mut self, ball_hit: api_models::BallHit) {
        if self.state.in_replay {
            return;
        }
        self.state.count_down = false;
        if let Some(timestamp) = self.state.timestamp {
            for player in ball_hit.players {
                let player_primary_id = self.get_player_primary_id(&player);

                self.stats.ball_hits.push(BallHitStatistic {
                    timestamp,
                    pre_hit_speed: ball_hit.ball.pre_hit_speed,
                    post_hit_speed: ball_hit.ball.post_hit_speed,
                    location: ball_hit.ball.location,
                    player_primary_id,
                })
            }
        }
    }

    fn get_player_primary_id(&mut self, player: &api_models::GamePlayer) -> String {
        let player_primary_id = self
            .get_player_stats(&player)
            .expect("Cannot find primary id for player")
            .primary_id
            .clone();
        player_primary_id
    }

    #[inline]
    fn get_player_stats_mut(
        &mut self,
        player: &api_models::GamePlayer,
    ) -> Option<&mut PlayerStats> {
        self.stats
            .teams
            .get_mut(&player.team_num)?
            .players
            .get_mut(&player.name)
    }

    #[inline]
    fn get_player_stats(&mut self, player: &api_models::GamePlayer) -> Option<&PlayerStats> {
        self.stats
            .teams
            .get(&player.team_num)?
            .players
            .get(&player.name)
    }

    fn insert_update_state(&mut self, update_state: api_models::UpdateState) {
        self.update_game_state(&update_state);
        self.update_game_stats(update_state);
    }

    #[inline]
    fn state_update_time_delta(&self) -> Option<i64> {
        Some(self.state.state_update_timestamp? - self.state.last_state_update_timestamp?)
    }

    fn update_game_state(&mut self, update_state: &api_models::UpdateState) {
        self.state.in_replay = update_state.game.b_replay;
        self.state.in_overtime = update_state.game.b_overtime;
        if self.state.in_replay {
            self.state.goal_scored = false;
        }
        self.update_non_replay_timestamps();
        // self.update_countdown(update_state);
    }

    fn update_non_replay_timestamps(&mut self) {
        if self.state.in_replay || self.state.count_down || self.state.goal_scored {
            // self.state.last_state_update_timestamp = None;
            self.state.state_update_timestamp = None;
            return;
        }

        self.state.last_state_update_timestamp = self.state.state_update_timestamp;
        self.state.state_update_timestamp = self.state.timestamp;
    }

    // fn update_countdown(&mut self, update_state: &UpdateState) {
    //     let anyone_moving_while_countdown_is_set = self.state.count_down
    //         && update_state
    //             .players
    //             .iter()
    //             .any(|player| player.speed.is_some_and(|speed| speed > 1f64));

    //     if anyone_moving_while_countdown_is_set {
    //         self.state.count_down = false;
    //     }
    // }

    fn update_game_stats(&mut self, update_state: api_models::UpdateState) {
        if update_state.game.b_replay {
            return;
        }
        if self.state.count_down {
            return;
        }
        if self.state.in_overtime {
            self.stats.had_overtime = true;
        }

        if let Some(delta) = self.state_update_time_delta() {
            self.stats.duration += delta;
        }

        if self.timeline_update_reasonable(&update_state) {
            self.update_timeline(&update_state);
        } else {
            self.stats.excluded_timeline_instants += 1;
        }

        self.stats.arena_name.get_or_insert(update_state.game.arena);

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

    fn get_or_create_team_stats_mut(&mut self, team: api_models::Team) -> &mut TeamStats {
        self.stats
            .teams
            .entry(team.team_num)
            .or_insert(TeamStats::new(
                team.name,
                team.color_primary,
                team.color_secondary,
            ))
    }

    fn get_or_create_player_stats_mut(&mut self, player: api_models::Player) -> &mut PlayerStats {
        self.stats
            .teams
            .get_mut(&player.team_num)
            .unwrap_or_else(|| panic!("Team doesn't exist for player {}", &player.name))
            .players
            .entry(player.name.clone())
            .or_insert(PlayerStats::new(player.name, player.primary_id))
    }

    fn timeline_update_reasonable(&self, update_state: &api_models::UpdateState) -> bool {
        const MIN_DELTA: f64 = 0.5;
        const RELATIVE_DELTA: f64 = 0.1;

        if let Some(last_timeline_instant) = self.stats.timeline.last() {
            let last_speed = last_timeline_instant.ball_state.speed;
            let speed = update_state.game.ball_state.speed;

            (speed - last_speed).abs() >= MIN_DELTA
        } else {
            true
        }
    }

    fn update_timeline(&mut self, update_state: &api_models::UpdateState) {
        if let Some(timestamp) = self.state.timestamp {
            self.stats.timeline.push(TimelineInstant {
                timestamp,
                ball_state: update_state.game.ball_state.clone(),
            });
        }
    }

    fn insert_crossbar_hit(&mut self, crossbar_hit: api_models::CrossbarHit) {
        if let Some(timestamp) = self.state.timestamp {
            if let Some(player_stats) =
                self.get_player_stats_mut(&crossbar_hit.ball_last_touch.player)
            {
                player_stats.crossbar_hits.push(CrossbarHitStatistic {
                    timestamp,
                    ball_speed: crossbar_hit.ball_speed,
                    impact_force: crossbar_hit.impact_force,
                    location: crossbar_hit.ball_location,
                    last_touch_speed: crossbar_hit.ball_last_touch.speed,
                });
            }
        }
    }

    fn insert_goal_scored(&mut self, goal_scored: api_models::GoalScored) {
        if self.state.in_replay {
            return;
        }
        self.state.goal_scored = true;

        if let Some(timestamp) = self.state.timestamp {
            let scorer_primary_id = self.get_player_primary_id(&goal_scored.scorer);
            let assister_primary_id = goal_scored
                .assister
                .and_then(|assister| Some(self.get_player_primary_id(&assister)));
            let last_touch_primary_id =
                self.get_player_primary_id(&goal_scored.ball_last_touch.player);

            self.stats.goal_details.push(GoalDetails {
                timestamp,
                goal_time: goal_scored.goal_time,
                impact_location: goal_scored.impact_location,
                goal_speed: goal_scored.goal_speed,
                last_touch_speed: goal_scored.ball_last_touch.speed,
                scorer_primary_id,
                assister_primary_id,
                last_touch_primary_id,
            });
        }
    }

    fn insert_clock_samples(&mut self, clock_updated_seconds: api_models::ClockUpdatedSeconds) {
        if let Some(timestamp) = self.state.timestamp {
            self.stats.clock_samples.push(ClockSample {
                timestamp,
                time_seconds: clock_updated_seconds.time_seconds,
                is_overtime: clock_updated_seconds.b_overtime,
            });
        }
    }

    fn insert_stat_feed_event(&mut self, statfeed_event: api_models::StatfeedEvent) {
        if self.state.in_replay {
            return;
        }

        let main_target_primary_id = self.get_player_primary_id(&statfeed_event.main_target);
        let secondary_target_primary_id = statfeed_event
            .secondary_target
            .and_then(|target| Some(self.get_player_primary_id(&target)));

        if let Some(timestamp) = self.state.timestamp {
            self.stats.statfeed_events.push(StatfeedEventStatistic {
                timestamp,
                event_name: statfeed_event.event_name,
                event_type: statfeed_event.event_type,
                main_target_primary_id,
                secondary_target_primary_id,
            });
        }
    }
}

fn update_core_player_stats(player: &api_models::Player, player_stats: &mut PlayerStats) {
    player_stats.score = player.score;
    player_stats.goals = player.goals;
    player_stats.shots = player.shots;
    player_stats.assists = player.assists;
    player_stats.saves = player.saves;
    player_stats.touches = player.touches;
    player_stats.car_touches = player.car_touches;
    player_stats.demos = player.demos;
    player_stats.shortcut = player.shortcut;
}

fn increment_counters(
    player_stats: &mut PlayerStats,
    player: &api_models::Player,
    time_delta: i64,
) {
    if player.b_boosting == Some(true) {
        player_stats
            .advanced_stats
            .get_or_insert_default()
            .time_boosting += time_delta;
    }
    if player.b_demolished == Some(true) {
        player_stats
            .advanced_stats
            .get_or_insert_default()
            .time_demolished += time_delta;
    }
    if player.b_on_ground == Some(true) {
        player_stats
            .advanced_stats
            .get_or_insert_default()
            .time_on_ground += time_delta;
    }
    if player.b_on_wall == Some(true) {
        player_stats
            .advanced_stats
            .get_or_insert_default()
            .time_on_wall += time_delta;
    }
    if player.b_powersliding == Some(true) {
        player_stats
            .advanced_stats
            .get_or_insert_default()
            .time_powersliding += time_delta;
    }
    if player.b_supersonic == Some(true) {
        player_stats
            .advanced_stats
            .get_or_insert_default()
            .time_supersonic += time_delta;
    }
}
