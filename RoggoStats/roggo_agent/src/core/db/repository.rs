use std::collections::HashMap;
use std::path::Path;

use rusqlite::{Connection, params};

use crate::core::api::dto::MatchDto;
use crate::core::bl::intermediate_models;
use crate::core::bl::query_models::MatchRow;
use crate::core::db::Result;

pub struct Repository {
    connection: Connection,
}

impl Repository {
    pub fn new(path: &Path) -> Result<Self> {
        let repo = Self::connect(path)?;
        repo.init()?;
        Ok(repo)
    }

    pub fn connect(path: &Path) -> Result<Self> {
        Ok(Self {
            connection: Connection::open(path)?,
        })
    }

    pub fn new_in_memory() -> Result<Self> {
        let repo = Self {
            connection: Connection::open_in_memory()?,
        };
        repo.init()?;

        Ok(repo)
    }

    fn init(&self) -> Result<()> {
        self.connection.pragma_update(None, "journal_mode", "WAL")?;
        self.connection.pragma_update(None, "busy_timeout", 5000)?;
        self.connection.pragma_update(None, "foreign_keys", "ON")?;

        self.connection
            .execute_batch(include_str!("sql/init.sql"))?;
        Ok(())
    }

    pub fn insert_game_stats(&mut self, stats: intermediate_models::GameStats) -> Result<()> {
        let match_guid = stats.match_guid;
        let mut player_ids = HashMap::new();
        let mut team_ids = HashMap::new();

        let tx = self.connection.transaction()?;
        insert_match(&stats, match_guid, &tx)?;

        for (team_num, team) in stats.teams {
            let team_id = insert_team(match_guid, &tx, team_num, &team)?;
            if team_ids.insert(team_num, team_id).is_some() {
                tracing::warn!(
                    "Cancel insert. Double team num detected in one game: {}",
                    team_num
                );
                return Ok(());
            }
            for (player_name, player) in &team.players {
                if player.primary_id.contains("Unknown") {
                    // Ignore Bots
                    continue;
                }

                upsert_global_player(&tx, player_name, &player)?;
                let player_id = insert_player(match_guid, &tx, team_id, player)?;
                if player_ids
                    .insert(player.primary_id.clone(), player_id)
                    .is_some()
                {
                    tracing::warn!(
                        "Cancel insert. Double primary id detected in one game: {}",
                        player.primary_id
                    );
                    return Ok(());
                }

                if let Some(advanced_player_stats) = &player.advanced_stats {
                    insert_player_stats(&tx, player_id, advanced_player_stats)?;
                }

                for crossbar_hit in &player.crossbar_hits {
                    insert_crossbar_hit(match_guid, &tx, player_id, crossbar_hit)?;
                }
            }
        }

        for clock_sample in stats.clock_samples {
            insert_clock_sample(match_guid, &tx, clock_sample)?;
        }

        for goal_details in stats.goal_details {
            insert_goal_details(match_guid, &player_ids, &tx, goal_details)?;
        }

        for ball_hit in &stats.ball_hits {
            if let Some(player_id) = player_ids.get(&ball_hit.player_primary_id) {
                let ball_hit_id = insert_ball_hit(match_guid, &tx, ball_hit)?;
                tx.execute(
                    include_str!("sql/insert/ball_hit_players.sql"),
                    params![ball_hit_id, player_id],
                )?;
            }
        }

        for statfeed_event in &stats.statfeed_events {
            insert_statfeed_event(match_guid, &player_ids, &tx, statfeed_event)?;
        }

        for timeline_instant in &stats.timeline {
            let team_id = team_ids.get(&timeline_instant.ball_state.team_num);

            tx.execute(
                include_str!("sql/insert/timeline_instant.sql"),
                params![
                    match_guid,
                    timeline_instant.timestamp,
                    team_id,
                    timeline_instant.ball_state.speed
                ],
            )?;
        }

        tx.commit()?;

        tracing::info!("Saved {} in database", match_guid);
        Ok(())
    }

    pub fn get_match(&self) -> Result<Vec<MatchRow>> {
        let mut stmt = self.connection.prepare(
            "
            select * from matches
            order by created_at_ms desc
            ",
        )?;

        let rows = stmt.query_map([], |row| {
            Ok(MatchRow {
                match_guid: row.get("match_guid")?,
                arena: row.get("arena")?,
                duration: row.get("duration")?,
                created_at: row.get("created_at_ms")?,
                ended_at: row.get("ended_at_ms")?,
                had_overtime: row.get("had_overtime")?,
                deleted: row.get("deleted")?,
            })
        })?;

        let mut matches = vec![];

        for row in rows {
            matches.push(row?);
        }

        Ok(matches)
    }
}

fn insert_statfeed_event(
    match_guid: uuid::Uuid,
    player_ids: &HashMap<String, i64>,
    tx: &rusqlite::Transaction<'_>,
    statfeed_event: &intermediate_models::StatfeedEventStatistic,
) -> Result<()> {
    Ok(
        if let Some(player_id) = player_ids.get(&statfeed_event.main_target_primary_id) {
            let secondary_target = match &statfeed_event.secondary_target_primary_id {
                Some(id) => Some(
                    player_ids
                        .get(id)
                        .expect("Secondary target not inserted for this statfeed event."),
                ),
                None => None,
            };

            tx.execute(
                include_str!("sql/insert/statfeed_events.sql"),
                params![
                    match_guid,
                    statfeed_event.timestamp,
                    statfeed_event.event_name,
                    statfeed_event.event_type,
                    player_id,
                    secondary_target
                ],
            )?;
        },
    )
}

fn insert_ball_hit(
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
    ball_hit: &intermediate_models::BallHitStatistic,
) -> Result<i64> {
    tx.execute(
        include_str!("sql/insert/ball_hits.sql"),
        params![
            match_guid,
            ball_hit.timestamp,
            ball_hit.pre_hit_speed,
            ball_hit.post_hit_speed,
            ball_hit.location.x,
            ball_hit.location.y,
            ball_hit.location.z,
        ],
    )?;
    Ok(tx.last_insert_rowid())
}

fn insert_goal_details(
    match_guid: uuid::Uuid,
    player_ids: &HashMap<String, i64>,
    tx: &rusqlite::Transaction<'_>,
    goal_details: intermediate_models::GoalDetails,
) -> Result<()> {
    let scorer_id = player_ids
        .get(&goal_details.scorer_primary_id)
        .expect("Scorer not inserted for this goal.");
    let assister_id = match goal_details.assister_primary_id {
        Some(id) => Some(
            player_ids
                .get(&id)
                .expect("Assister not inserted for this goal."),
        ),
        None => None,
    };
    let last_touch_player_id = player_ids
        .get(&goal_details.last_touch_primary_id)
        .expect("Last touch player not inserted for this goal.");
    tx.execute(
        include_str!("sql/insert/goal_details.sql"),
        params![
            match_guid,
            goal_details.timestamp,
            goal_details.goal_time,
            goal_details.impact_location.x,
            goal_details.impact_location.y,
            goal_details.impact_location.z,
            goal_details.goal_speed,
            goal_details.last_touch_speed,
            scorer_id,
            assister_id,
            last_touch_player_id,
        ],
    )?;
    Ok(())
}

fn insert_crossbar_hit(
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
    player_id: i64,
    crossbar_hit: &intermediate_models::CrossbarHitStatistic,
) -> Result<()> {
    tx.execute(
        include_str!("sql/insert/crossbar_hits.sql"),
        params![
            match_guid,
            crossbar_hit.timestamp,
            crossbar_hit.ball_speed,
            crossbar_hit.impact_force,
            crossbar_hit.location.x,
            crossbar_hit.location.y,
            crossbar_hit.location.z,
            crossbar_hit.last_touch_speed,
            player_id,
        ],
    )?;
    Ok(())
}

fn insert_clock_sample(
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
    clock_sample: intermediate_models::ClockSample,
) -> Result<()> {
    tx.execute(
        include_str!("sql/insert/clock_samples.sql"),
        params![
            match_guid,
            clock_sample.timestamp,
            clock_sample.time_seconds,
            clock_sample.is_overtime,
        ],
    )?;
    Ok(())
}

fn insert_player_stats(
    tx: &rusqlite::Transaction<'_>,
    player_id: i64,
    advanced_player_stats: &intermediate_models::AdvancedPlayerStats,
) -> Result<()> {
    tx.execute(
        include_str!("sql/insert/player_stats.sql"),
        params![
            player_id,
            advanced_player_stats.time_boosting,
            advanced_player_stats.time_demolished,
            advanced_player_stats.time_on_ground,
            advanced_player_stats.time_on_wall,
            advanced_player_stats.time_powersliding,
            advanced_player_stats.time_supersonic,
        ],
    )?;
    Ok(())
}

fn insert_player(
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
    team_id: i64,
    player: &intermediate_models::PlayerStats,
) -> Result<i64> {
    tx.execute(
        include_str!("sql/insert/player.sql"),
        params![
            match_guid,
            team_id,
            player.primary_id,
            player.name,
            player.shortcut,
            player.score,
            player.goals,
            player.shots,
            player.assists,
            player.saves,
            player.touches,
            player.car_touches,
            player.demos,
        ],
    )?;
    Ok(tx.last_insert_rowid())
}

fn upsert_global_player(
    tx: &rusqlite::Transaction<'_>,
    player_name: &str,
    player: &intermediate_models::PlayerStats,
) -> Result<()> {
    tx.execute(
        include_str!("sql/insert/upsert_global_player.sql"),
        params![player.primary_id, player_name],
    )?;
    Ok(())
}

fn insert_team(
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
    team_num: u8,
    team: &intermediate_models::TeamStats,
) -> Result<i64> {
    tx.execute(
        include_str!("sql/insert/team.sql"),
        params![
            match_guid,
            team.name.clone(),
            team_num,
            team.score,
            team.color_primary,
            team.color_secondary
        ],
    )?;
    Ok(tx.last_insert_rowid())
}

fn insert_match(
    stats: &intermediate_models::GameStats,
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
) -> Result<()> {
    tx.execute(
        include_str!("sql/insert/match.sql"),
        params![
            match_guid,
            stats.arena_name,
            stats.duration,
            stats.created_at_timestamp,
            stats.ended_at_timestamp,
            stats.had_overtime,
            false
        ],
    )?;
    Ok(())
}
