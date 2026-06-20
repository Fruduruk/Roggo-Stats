use std::collections::HashMap;

use uuid::Uuid;

use crate::core::{
    bl::{
        Error, Result,
        intermediate_models::{
            BallHitStatistic, ClockSample, CrossbarHitStatistic, GameState, GameStats, GoalDetails,
            PlayerStats, StatSnapshot, StatfeedEventStatistic, TimelineInstant,
        },
    },
    rl_api::models::{
        BallHit, ClockUpdatedSeconds, CrossbarHit, Event, GoalScored, Player,
        StatfeedEvent, UpdateState,
    },
};

#[derive(Debug)]
pub struct GameStatCollector {
    insert_count: u64,
    stats: GameStats,
    state: GameState,
    ball_hit_buffer: Vec<(i64, BallHit)>,
    crossbar_hit_buffer: Vec<(i64, CrossbarHit)>,
    statfeed_event_buffer: Vec<(i64, StatfeedEvent)>,
    goal_scored_buffer: Vec<(i64, GoalScored)>,
    player_stats_buffer: HashMap<String, Vec<(i64, StatSnapshot)>>,
}

impl GameStatCollector {
    pub fn new(match_guid: Uuid, timestamp: i64) -> Self {
        Self {
            insert_count: 0,
            stats: GameStats::new(match_guid, timestamp),
            state: GameState::default(),
            ball_hit_buffer: vec![],
            crossbar_hit_buffer: vec![],
            statfeed_event_buffer: vec![],
            goal_scored_buffer: vec![],
            player_stats_buffer: HashMap::new(),
        }
    }

    pub fn get_match_guid(&self) -> Uuid {
        self.stats.match_guid
    }

    pub fn export(self) -> (GameStats, Vec<Error>) {
        let Self {
            insert_count: _,
            mut stats,
            state: _,
            ball_hit_buffer,
            crossbar_hit_buffer,
            statfeed_event_buffer,
            goal_scored_buffer,
            player_stats_buffer: _,
        } = self;

        let mut errors = vec![];

        for (timestamp, ball_hit) in ball_hit_buffer {
            if let Err(error) = insert_ball_hit(&mut stats, timestamp, ball_hit) {
                errors.push(error);
            }
        }
        for (timestamp, crossbar_hit) in crossbar_hit_buffer {
            if let Err(error) = insert_crossbar_hit(&mut stats, timestamp, crossbar_hit) {
                errors.push(error);
            }
        }
        for (timestamp, statfeed_event) in statfeed_event_buffer {
            if let Err(error) = insert_stat_feed_event(&mut stats, timestamp, statfeed_event) {
                errors.push(error);
            }
        }
        for (timestamp, goal_scored) in goal_scored_buffer {
            if let Err(error) = insert_goal_scored(&mut stats, timestamp, goal_scored) {
                errors.push(error);
            }
        }

        // for (player_id, snapshots) in player_stats_buffer {
        //     let count = snapshots.len() as f64;
        //     let average_speed = snapshots
        //         .iter()
        //         .map(|(_, snapshot)| snapshot.speed)
        //         .sum::<f64>()
        //         / count;
        //     let average_boost = snapshots
        //         .iter()
        //         .map(|(_, snapshot)| snapshot.boost)
        //         .sum::<f64>()
        //         / count;
        //     tracing::debug!(
        //         "average stats for {}: speed {}, boost {}",
        //         player_id,
        //         average_speed,
        //         average_boost
        //     );
        // }

        (stats, errors)
    }

    pub fn is_finished(&self) -> bool {
        self.state.finished
    }

    pub fn insert(&mut self, timestamp: i64, event: Event) {
        self.insert_count += 1;
        self.state.timestamp = Some(timestamp);
        // println!("{:#?}", self.state);
        match event {
            Event::UpdateState(update_state) => self.insert_update_state(update_state),
            Event::BallHit(ball_hit) => self.push_ball_hit(ball_hit),
            Event::ClockUpdatedSeconds(clock_updated_seconds) => {
                self.insert_clock_samples(clock_updated_seconds)
            }
            Event::CountdownBegin(_) => {
                self.state.count_down = true;
                self.state.goal_scored = false;
            }
            Event::CrossbarHit(crossbar_hit) => self.push_crossbar_hit(crossbar_hit),
            Event::GoalScored(goal_scored) => self.push_goal_scored(goal_scored),
            Event::MatchDestroyed(_) | Event::PodiumStart(_) | Event::MatchEnded(_) => {
                self.stats.ended_at_timestamp = timestamp;
                self.state.finished = true;
            }
            Event::RoundStarted(_) => {
                self.state.round_started_once = true;
            },
            Event::StatfeedEvent(statfeed_event) => self.push_stat_feed_event(statfeed_event),
            _ => return,
        }
    }

    fn push_ball_hit(&mut self, ball_hit: BallHit) {
        if self.state.in_replay {
            return;
        }

        self.state.match_started = true;
        self.state.count_down = false;

        if let Some(timestamp) = self.state.timestamp {
            self.ball_hit_buffer.push((timestamp, ball_hit));
        }
    }

    #[inline]
    fn insert_update_state(&mut self, update_state: UpdateState) {
        self.update_game_state(&update_state);
        self.update_game_stats(update_state);
    }

    fn update_game_state(&mut self, update_state: &UpdateState) {
        self.state.in_replay = update_state.game.b_replay;
        self.state.in_overtime = update_state.game.b_overtime;
        if self.state.in_replay {
            self.state.goal_scored = false;
        }
        self.update_non_replay_timestamps();
    }

    fn update_non_replay_timestamps(&mut self) {
        if self.state.in_replay || self.state.goal_scored || !self.state.round_started_once {
            self.state.state_update_timestamp = None;
            return;
        }

        self.state.last_state_update_timestamp = self.state.state_update_timestamp;
        self.state.state_update_timestamp = self.state.timestamp;
    }

    fn update_game_stats(&mut self, update_state: UpdateState) {
        let round_started_once = self.state.round_started_once;
        if round_started_once {
            if update_state.game.b_replay {
                return;
            }
            if self.state.count_down {
                return;
            }
            if !self.state.match_started {
                return;
            }
        }

        if self.state.in_overtime {
            self.stats.had_overtime = true;
        }

        if let Some(delta) = self.state.state_update_time_delta() {
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
            self.stats.get_or_create_team_stats_mut(team).score = score;
        }

        for player in update_state.players {
            let delta = self.state.state_update_time_delta();

            let player_stats = self.get_or_create_player_stats_mut(player.clone());

            update_core_player_stats(&player, player_stats);
            if let Some(delta) = delta {
                if round_started_once {
                    // Don't count, if round never started once.
                    increment_counters(player_stats, &player, delta);
                    if let Some(timestamp) = self.state.timestamp {
                        if let (Some(boost), Some(speed)) = (player.boost, player.speed) {
                            let buffer = self
                                .player_stats_buffer
                                .entry(player.primary_id)
                                .or_default();
                            buffer.push((timestamp, StatSnapshot { speed, boost }));
                        }
                    }
                }
            }
        }
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

    fn timeline_update_reasonable(&self, update_state: &UpdateState) -> bool {
        const MIN_DELTA: f64 = 0.5;
        // const RELATIVE_DELTA: f64 = 0.1;

        if let Some(last_timeline_instant) = self.stats.timeline.last() {
            let last_speed = last_timeline_instant.ball_state.speed;
            let speed = update_state.game.ball_state.speed;

            (speed - last_speed).abs() >= MIN_DELTA
        } else {
            true
        }
    }

    fn update_timeline(&mut self, update_state: &UpdateState) {
        if let Some(timestamp) = self.state.timestamp {
            self.stats.timeline.push(TimelineInstant {
                timestamp,
                ball_state: update_state.game.ball_state.clone(),
            });
        }
    }

    fn push_crossbar_hit(&mut self, crossbar_hit: CrossbarHit) {
        if let Some(timestamp) = self.state.timestamp {
            self.crossbar_hit_buffer.push((timestamp, crossbar_hit));
        }
    }

    fn push_goal_scored(&mut self, goal_scored: GoalScored) {
        if self.state.in_replay {
            return;
        }
        self.state.goal_scored = true;

        if let Some(timestamp) = self.state.timestamp {
            self.goal_scored_buffer.push((timestamp, goal_scored));
        }
    }

    fn insert_clock_samples(&mut self, clock_updated_seconds: ClockUpdatedSeconds) {
        if let Some(timestamp) = self.state.timestamp {
            self.stats.clock_samples.push(ClockSample {
                timestamp,
                time_seconds: clock_updated_seconds.time_seconds,
                is_overtime: clock_updated_seconds.b_overtime,
            });
        }
    }

    fn push_stat_feed_event(&mut self, statfeed_event: StatfeedEvent) {
        if self.state.in_replay {
            return;
        }

        if let Some(timestamp) = self.state.timestamp {
            self.statfeed_event_buffer.push((timestamp, statfeed_event));
        }
    }
}

fn insert_goal_scored(
    stats: &mut GameStats,
    timestamp: i64,
    goal_scored: GoalScored,
) -> Result<()> {
    let scorer_primary_id = stats
        .get_player_primary_id(&goal_scored.scorer)
        .ok_or_else(|| {
            Error::InsertionFailed(format!(
                "GoalScored: cannot find primary id for scorer {}",
                &goal_scored.scorer.name
            ))
        })?;

    let assister_primary_id = if let Some(assister) = goal_scored.assister.clone() {
        if let Some(id) = stats.get_player_primary_id(&assister) {
            Some(id)
        } else {
            return Err(Error::InsertionFailed(format!(
                "GoalScored: cannot find primary id for assister {:#?}",
                &goal_scored.assister
            )));
        }
    } else {
        None
    };

    let last_touch_primary_id = stats
        .get_player_primary_id(&goal_scored.ball_last_touch.player)
        .ok_or_else(|| {
            Error::InsertionFailed(format!(
                "GoalScored: cannot find primary id for last touch player {}",
                &goal_scored.ball_last_touch.player.name
            ))
        })?;

    stats.goal_details.push(GoalDetails {
        timestamp,
        goal_time: goal_scored.goal_time,
        impact_location: goal_scored.impact_location,
        goal_speed: goal_scored.goal_speed,
        last_touch_speed: goal_scored.ball_last_touch.speed,
        scorer_primary_id,
        assister_primary_id,
        last_touch_primary_id,
    });
    Ok(())
}

fn insert_stat_feed_event(
    stats: &mut GameStats,
    timestamp: i64,
    statfeed_event: StatfeedEvent,
) -> Result<()> {
    let main_target_primary_id = stats
        .get_player_primary_id(&statfeed_event.main_target)
        .ok_or_else(|| {
            Error::InsertionFailed(format!(
                "StatfeedEvent: cannot find primary id for main target {}",
                &statfeed_event.main_target.name
            ))
        })?;

    let secondary_target_primary_id =
        if let Some(secondary_target) = statfeed_event.secondary_target {
            if let Some(id) = stats.get_player_primary_id(&secondary_target) {
                Some(id)
            } else {
                return Err(Error::InsertionFailed(format!(
                    "StatfeedEvent: cannot find primary id for secondary target {}",
                    &secondary_target.name
                )));
            }
        } else {
            None
        };

    stats.statfeed_events.push(StatfeedEventStatistic {
        timestamp,
        event_name: statfeed_event.event_name,
        event_type: statfeed_event.event_type,
        main_target_primary_id,
        secondary_target_primary_id,
    });
    Ok(())
}

fn insert_crossbar_hit(
    stats: &mut GameStats,
    timestamp: i64,
    crossbar_hit: CrossbarHit,
) -> Result<()> {
    stats
        .get_player_stats_mut(&crossbar_hit.ball_last_touch.player)
        .ok_or_else(|| Error::InsertionFailed("CrossbarHit".into()))?
        .crossbar_hits
        .push(CrossbarHitStatistic {
            timestamp,
            ball_speed: crossbar_hit.ball_speed,
            impact_force: crossbar_hit.impact_force,
            location: crossbar_hit.ball_location,
            last_touch_speed: crossbar_hit.ball_last_touch.speed,
        });
    Ok(())
}

fn insert_ball_hit(stats: &mut GameStats, timestamp: i64, ball_hit: BallHit) -> Result<()> {
    for player in ball_hit.players {
        let player_primary_id = stats
            .get_player_primary_id(&player)
            .ok_or_else(|| Error::InsertionFailed("BallHit".into()))?;

        stats.ball_hits.push(BallHitStatistic {
            timestamp,
            pre_hit_speed: ball_hit.ball.pre_hit_speed,
            post_hit_speed: ball_hit.ball.post_hit_speed,
            location: ball_hit.ball.location,
            player_primary_id,
        })
    }

    Ok(())
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
    player_stats.shortcut = player.shortcut;
}

fn increment_counters(player_stats: &mut PlayerStats, player: &Player, time_delta: i64) {
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
