use std::path::Path;

use rusqlite::{Connection, Result, params};

use crate::core::models::intermediate_models::{GameStats, PlayerStats};

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

    pub fn insert_game_stats(&mut self, stats: GameStats) -> Result<()> {
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
                insert_player(match_guid, &tx, team_num, player)?;
            }
        }

        tx.commit()?;
        Ok(())
    }
}

fn insert_player(match_guid: uuid::Uuid, tx: &rusqlite::Transaction<'_>, team_num: u8, player: PlayerStats) -> Result<(), rusqlite::Error> {
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
    Ok(())
}

fn upsert_global_player(tx: &rusqlite::Transaction<'_>, player_name: String, player: &PlayerStats) -> Result<(), rusqlite::Error> {
    println!("Upserting global player...");
    
    tx.execute(
        include_str!("sql/upsert_global_player.sql"),
        params![player.primary_id, player_name],
    )?;
    Ok(())
}

fn insert_team(match_guid: uuid::Uuid, tx: &rusqlite::Transaction<'_>, team_num: u8, team: &crate::core::models::intermediate_models::TeamStats) -> Result<(), rusqlite::Error> {
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

fn insert_match(stats: &GameStats, match_guid: uuid::Uuid, tx: &rusqlite::Transaction<'_>) -> Result<(), rusqlite::Error> {
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
