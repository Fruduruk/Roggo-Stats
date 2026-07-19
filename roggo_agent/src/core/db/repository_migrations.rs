use std::time::Instant;

use rusqlite::{Transaction, params};

use crate::AGENT_VERSION;
use crate::core::db::{Error, Repository, Result, get_current_timestamp};

impl Repository {
    pub fn ensure_created_and_migrated(path: &std::path::Path) -> Result<()> {
        let mut repo = Self::connect(path)?;
        repo.connection.pragma_update(None, "foreign_keys", "ON")?;

        repo.init()?;
        repo.migrate_v1().map_err(|sql_error| Error::MigrationError(format!("Migration 1 failed: {:#?}",sql_error)))?;

        Ok(())
    }

    fn init(&mut self) -> Result<()> {
        // anything higher than 0 means the database is initialized
        if self.get_version().unwrap_or(0) > 0 {
            tracing::info!("Database is already initialized");
            return Ok(());
        }

        self.connection.pragma_update(None, "journal_mode", "WAL")?;
        self.connection.pragma_update(None, "busy_timeout", 5000)?;
        self.connection
            .execute_batch(include_str!("sql/init.sql"))?;

        tracing::info!("Database initialized successfully.");
        Ok(())
    }

    pub fn get_version(&self) -> Result<i64> {
        let mut stmt = self.connection.prepare(
            "
            select version
            from migration_history
            order by version desc
            limit 1
            ",
        )?;

        Ok(stmt.query_row([], |row| Ok(row.get("version")?))?)
    }

    

    fn migrate_v1(&mut self) -> Result<()> {
        const MIGRATION_VERSION: i64 = 1;
        if self.get_version().unwrap_or(0) >= MIGRATION_VERSION {
            info_already_migrated(MIGRATION_VERSION);
            return Ok(());
        }

        let now = Instant::now();

        // turn foreign_keys temporarily off so the keys won't follow the rename step
        self.connection.pragma_update(None, "foreign_keys", "OFF")?;

        let tx = self.connection.transaction()?;
        tx.execute_batch(include_str!("sql/migrations/v1.sql"))?;

        check_foreign_keys(&tx)?;

        add_migration_history_entry(
            &tx,
            MIGRATION_VERSION,
            "
                    1.1: change global_players attribute last_username from integer to text
                    1.2: drop timeline table because not in use, saving 85% file size
                    1.3: create migration history table
                    1.4: update match metadata versions to 0
                ",
            elapsed_ms(now)?,
        )?;
        tx.commit()?;

        self.connection.pragma_update(None, "foreign_keys", "ON")?;

        // clear space because table was dropped
        self.connection.execute_batch("VACUUM;")?;

        info_successfully_migrated(MIGRATION_VERSION);
        Ok(())
    }
}

fn info_already_migrated(version: i64) {
    tracing::info!("Database is already migrated to version {version}");
}

fn info_successfully_migrated(version: i64) {
    tracing::info!("Database successfully migrated to version {version}");
}

fn check_foreign_keys(tx: &Transaction<'_>) -> Result<()> {
        let mut stmt = tx.prepare("pragma foreign_key_check;")?;

        let violations = stmt
            .query_map([], |row| {
                let table: String = row.get(0)?;

                Ok(format!(
                    "{table} has wrong foreign keys"
                ))
            })?
            .collect::<std::result::Result<Vec<_>, _>>()?;

        if !violations.is_empty() {
            return Err(Error::GeneralError(format!(
                "Foreign key check failed: {}",
                violations.join("\n")
            )));
        }

        Ok(())
    }

pub fn add_migration_history_entry(
    tx: &Transaction<'_>,
    version: i64,
    name: &str,
    execution_time_ms: i64,
) -> Result<()> {
    let timestamp_ms = get_current_timestamp()?;
    tx.execute(
        "
                insert into migration_history (
                    version,
                    name,
                    applied_at_ms,
                    agent_version,
                    execution_time_ms
                )
                values (?1, ?2, ?3, ?4, ?5);
                ",
        params![
            version,
            name,
            timestamp_ms,
            AGENT_VERSION,
            execution_time_ms
        ],
    )?;
    Ok(())
}

fn elapsed_ms(start: Instant) -> Result<i64> {
    i64::try_from(start.elapsed().as_millis())
        .map_err(|_| Error::GeneralError("Execution time millis does not fit into i64".into()))
}
