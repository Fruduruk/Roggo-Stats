use std::collections::HashMap;
use std::path::Path;

use rusqlite::{Connection, Transaction, params};

use crate::core::api::dto::MatchDto;
use crate::core::bl::query_models::MatchRow;
use crate::core::bl::{self, intermediate_models};
use crate::core::db::{Error, Result};

const SCHEMA_VERSION: i64 = 1;
const AGENT_VERSION: &str = "0.1.0";

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

    pub fn insert_game_stats(
        &mut self,
        stats: intermediate_models::GameStats,
        errors: Vec<bl::Error>,
    ) -> Result<()> {
        let mut player_ids = HashMap::new();
        let mut team_ids = HashMap::new();

        let tx = self.connection.transaction()?;
        let match_id = insert_match(&stats, stats.match_guid, &tx)?;
        insert_metadata(match_id, &tx)?;

        for error in errors {
            insert_error(match_id, error, &tx)?;
        }

        for (team_num, team) in stats.teams {
            let team_id = insert_team(match_id, &tx, team_num, &team)?;
            if team_ids.insert(team_num, team_id).is_some() {
                let message = format!(
                    "Cancel insert. Double team num detected in one game: {}",
                    team_num
                );
                return Err(Error::GeneralError(message));
            }
            for (player_name, player) in &team.players {
                if player.primary_id.contains("Unknown") {
                    // Ignore Bots
                    continue;
                }

                let global_player_id = upsert_global_player(&tx, player_name, &player)?;
                let player_id = insert_player(&tx, match_id, team_id, global_player_id, player)?;
                if player_ids
                    .insert(player.primary_id.clone(), player_id)
                    .is_some()
                {
                    let message = format!(
                        "Cancel insert. Double primary id detected in one game: {}",
                        player.primary_id
                    );
                    return Err(Error::GeneralError(message));
                }

                if let Some(advanced_player_stats) = &player.advanced_stats {
                    insert_player_stats(&tx, player_id, advanced_player_stats)?;
                }

                for crossbar_hit in &player.crossbar_hits {
                    insert_crossbar_hit(&tx, match_id, player_id, crossbar_hit)?;
                }
            }
        }

        for clock_sample in stats.clock_samples {
            insert_clock_sample(match_id, &tx, clock_sample)?;
        }

        for goal_details in stats.goal_details {
            insert_goal_details(match_id, &player_ids, &tx, goal_details)?;
        }

        for ball_hit in &stats.ball_hits {
            if let Some(player_id) = player_ids.get(&ball_hit.player_primary_id) {
                let ball_hit_id = insert_ball_hit(match_id, &tx, ball_hit)?;
                tx.execute(
                    "
                        insert into ball_hit_players (
                            ball_hit_id,
                            player_id
                        )
                        values (?1,?2);
                    ",
                    params![ball_hit_id, player_id],
                )?;
            }
        }

        for statfeed_event in &stats.statfeed_events {
            insert_statfeed_event(match_id, &player_ids, &tx, statfeed_event)?;
        }

        for timeline_instant in &stats.timeline {
            let team_id = team_ids.get(&timeline_instant.ball_state.team_num);

            tx.execute(
                "
                    insert into timeline (
                        match_id,
                        timestamp_ms,
                        last_touch_team_id,
                        ball_speed
                    )
                    values (?1,?2,?3,?4);
                ",
                params![
                    match_id,
                    timeline_instant.timestamp,
                    team_id,
                    timeline_instant.ball_state.speed
                ],
            )?;
        }

        tx.commit()?;

        tracing::info!("Saved {} in database", stats.match_guid);
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
    match_id: i64,
    player_ids: &HashMap<String, i64>,
    tx: &Transaction<'_>,
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
                "
                    insert into statfeed_events (
                        match_id,
                        timestamp_ms,
                        event_name,
                        event_type,
                        main_target_player_id,
                        secondary_target_player_id
                    )
                    values(?1,?2,?3,?4,?5,?6);
                ",
                params![
                    match_id,
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
    match_id: i64,
    tx: &Transaction<'_>,
    ball_hit: &intermediate_models::BallHitStatistic,
) -> Result<i64> {
    tx.execute(
        "
            insert into ball_hits (
                match_id,
                timestamp_ms,
                pre_hit_speed,
                post_hit_speed,
                x,
                y,
                z
            )
            values(?1,?2,?3,?4,?5,?6,?7);
        ",
        params![
            match_id,
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
    match_id: i64,
    player_ids: &HashMap<String, i64>,
    tx: &Transaction<'_>,
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
        "
            insert into goal_details (
                match_id,
                timestamp_ms,
                goal_time,
                impact_x,
                impact_y,
                impact_z,
                goal_speed,
                scorer_player_id,
                assister_player_id,
                last_touch_player_id,
                last_touch_speed
            )
            values(?1,?2,?3,?4,?5,?6,?7,?8,?9,?10,?11);
        ",
        params![
            match_id,
            goal_details.timestamp,
            goal_details.goal_time,
            goal_details.impact_location.x,
            goal_details.impact_location.y,
            goal_details.impact_location.z,
            goal_details.goal_speed,
            scorer_id,
            assister_id,
            last_touch_player_id,
            goal_details.last_touch_speed,
        ],
    )?;
    Ok(())
}

fn insert_crossbar_hit(
    tx: &Transaction<'_>,
    match_id: i64,
    last_touch_player_id: i64,
    crossbar_hit: &intermediate_models::CrossbarHitStatistic,
) -> Result<()> {
    tx.execute(
        "
            insert into crossbar_hits (
                match_id,
                timestamp_ms,
                ball_speed,
                impact_force,
                x,
                y,
                z,
                last_touch_speed,
                last_touch_player_id
            )
            values(?1,?2,?3,?4,?5,?6,?7,?8,?9);
        ",
        params![
            match_id,
            crossbar_hit.timestamp,
            crossbar_hit.ball_speed,
            crossbar_hit.impact_force,
            crossbar_hit.location.x,
            crossbar_hit.location.y,
            crossbar_hit.location.z,
            crossbar_hit.last_touch_speed,
            last_touch_player_id,
        ],
    )?;
    Ok(())
}

fn insert_clock_sample(
    match_id: i64,
    tx: &Transaction<'_>,
    clock_sample: intermediate_models::ClockSample,
) -> Result<()> {
    tx.execute(
        "
            insert into clock_samples (
                match_id,
                timestamp_ms,
                time_seconds,
                is_overtime
            )
            values(?1,?2,?3,?4);
        ",
        params![
            match_id,
            clock_sample.timestamp,
            clock_sample.time_seconds,
            clock_sample.is_overtime,
        ],
    )?;
    Ok(())
}

fn insert_player_stats(
    tx: &Transaction<'_>,
    player_id: i64,
    advanced_player_stats: &intermediate_models::AdvancedPlayerStats,
) -> Result<()> {
    tx.execute(
        "
            insert into player_stats (
                player_id,
                time_boosting,
                time_demolished,
                time_on_ground,
                time_on_wall,
                time_powersliding,
                time_supersonic
            )
            values(?1,?2,?3,?4,?5,?6,?7);
        ",
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
    tx: &Transaction<'_>,
    match_id: i64,
    team_id: i64,
    global_player_id: i64,
    player: &intermediate_models::PlayerStats,
) -> Result<i64> {
    tx.execute(
        "
            insert into players (
                match_id,
                team_id,
                global_player_id,
                display_name,
                shortcut,
                score,
                goals,
                shots,
                assists,
                saves,
                touches,
                car_touches,
                demos
            )
            values(?1,?2,?3,?4,?5,?6,?7,?8,?9,?10,?11,?12,?13);
        ",
        params![
            match_id,
            team_id,
            global_player_id,
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
    tx: &Transaction<'_>,
    username: &str,
    player: &intermediate_models::PlayerStats,
) -> Result<i64> {
    tx.execute(
        "
            insert into global_players (primary_id, last_username)
            values (?1,?2)
            on conflict(primary_id) do
            update set last_username = excluded.last_username;
        ",
        params![player.primary_id, username],
    )?;
    Ok(tx.last_insert_rowid())
}

fn insert_team(
    match_id: i64,
    tx: &Transaction<'_>,
    team_num: u8,
    team: &intermediate_models::TeamStats,
) -> Result<i64> {
    tx.execute(
        "
        insert into teams (
            match_id,
            team_num,
            name,
            score,
            color_primary,
            color_secondary
        )
        values (?1,?2,?3,?4,?5,?6);
        ",
        params![
            match_id,
            team_num,
            team.name.clone(),
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
    tx: &Transaction<'_>,
) -> Result<i64> {
    tx.execute(
        "
        insert into matches (
            match_guid,
            arena,
            duration,
            created_at_ms,
            ended_at_ms,
            had_overtime,
            deleted
        )
        values (?1,?2,?3,?4,?5,?6,?7);
        ",
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
    Ok(tx.last_insert_rowid())
}

fn insert_metadata(match_id: i64, tx: &Transaction<'_>) -> Result<()> {
    let timestamp_ms = i64::try_from(
        std::time::SystemTime::now()
            .duration_since(std::time::UNIX_EPOCH)
            .map_err(|_| Error::GeneralError("System time is before UNIX_EPOCH".into()))?
            .as_millis(),
    )
    .map_err(|_| Error::GeneralError("System time millis does not fit into i64".into()))?;

    tx.execute(
        "
        insert into match_metadata (
            match_id,
            schema_version,
            agent_version,
            saved_at_ms
        )
        values (?1,?2,?3,?4);
        ",
        params![match_id, SCHEMA_VERSION, AGENT_VERSION, timestamp_ms],
    )?;
    Ok(())
}

fn insert_error(match_id: i64, error: bl::Error, tx: &Transaction<'_>) -> Result<()> {
    tx.execute(
        "
        insert into errors (
            match_id,
            error
        )
        values (?1,?2);
    ",
        params![match_id, error.to_string()],
    )?;
    Ok(())
}
