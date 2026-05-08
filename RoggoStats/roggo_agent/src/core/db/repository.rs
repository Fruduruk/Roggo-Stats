use std::path::Path;

use rusqlite::{Connection, Result, params};

use crate::core::models::intermediate_models;

pub struct Repository {
    connection: Connection,
}

impl Repository {
    pub fn new(path: impl AsRef<Path>) -> Result<Self> {
        let repo = Self {
            connection: Connection::open(path)?,
        };
        repo.init()?;

        Ok(repo)
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
        let tx = self.connection.transaction()?;
        insert_match(&stats, match_guid, &tx)?;

        for (team_num, team) in stats.teams {
            insert_team(match_guid, &tx, team_num, &team)?;
            for (player_name, player) in team.players {
                if player.primary_id.contains("Unknown") {
                    // Ignore Bots
                    continue;
                }
                upsert_global_player(&tx, player_name, &player)?;
                let player_id = insert_player(match_guid, &tx, team_num, &player)?;
                if let Some(advanced_player_stats) = &player.advanced_stats {
                    insert_player_stats(&tx, player_id, advanced_player_stats)?;
                }

                println!("Inserting crossbar hits...");

                for crossbar_hit in &player.crossbar_hits {
                    insert_crossbar_hit(match_guid, &tx, player_id, crossbar_hit)?;
                }
            }
        }

        println!("Inserting clock samples...");

        for clock_sample in stats.clock_samples {
            insert_clock_sample(match_guid, &tx, clock_sample)?;
        }

        tx.commit()?;
        Ok(())
    }
}

fn insert_crossbar_hit(match_guid: uuid::Uuid, tx: &rusqlite::Transaction<'_>, player_id: i64, crossbar_hit: &intermediate_models::CrossbarHitStatistic) -> Result<(), rusqlite::Error> {
    tx.execute(
        include_str!("sql/insert_crossbar_hits.sql"),
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
) -> Result<(), rusqlite::Error> {
    tx.execute(
        include_str!("sql/insert_clock_samples.sql"),
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
) -> Result<(), rusqlite::Error> {
    println!("Inserting player stats...");

    tx.execute(
        include_str!("sql/insert_player_stats.sql"),
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
    team_num: u8,
    player: &intermediate_models::PlayerStats,
) -> Result<i64, rusqlite::Error> {
    println!("Inserting player...");
    tx.execute(
        include_str!("sql/insert_player.sql"),
        params![
            match_guid,
            team_num,
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
    player_name: String,
    player: &intermediate_models::PlayerStats,
) -> Result<(), rusqlite::Error> {
    println!("Upserting global player...");

    tx.execute(
        include_str!("sql/upsert_global_player.sql"),
        params![player.primary_id, player_name],
    )?;
    Ok(())
}

fn insert_team(
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
    team_num: u8,
    team: &crate::core::models::intermediate_models::TeamStats,
) -> Result<(), rusqlite::Error> {
    println!("Inserting team...");

    tx.execute(
        include_str!("sql/insert_team.sql"),
        params![
            match_guid,
            team.name.clone(),
            team_num,
            team.score,
            team.color_primary,
            team.color_secondary
        ],
    )?;
    Ok(())
}

fn insert_match(
    stats: &intermediate_models::GameStats,
    match_guid: uuid::Uuid,
    tx: &rusqlite::Transaction<'_>,
) -> Result<(), rusqlite::Error> {
    println!("Inserting match...");

    tx.execute(
        include_str!("sql/insert_match.sql"),
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
