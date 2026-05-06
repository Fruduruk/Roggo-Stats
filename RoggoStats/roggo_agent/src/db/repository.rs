use std::path::Path;

use rusqlite::{Connection, Result, params};

use crate::core::models::game_stats::GameStats;

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

        self.connection
            .execute_batch(include_str!("sql/init.sql"))?;
        Ok(())
    }

    pub fn insert_match(&mut self, stats: GameStats) -> Result<()> {
        let match_guid = stats.match_guid;
        let tx = self.connection.transaction()?;
        tx.execute(
            include_str!("sql/insert_match.sql"),
            params![
                match_guid,
                stats.arena_name,
                stats.created_at_timestamp,
                stats.ended_at_timestamp,
                stats.had_overtime
            ],
        )?;
        tx.commit()?;
        Ok(())
    }
}
