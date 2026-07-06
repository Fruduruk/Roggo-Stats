pub mod repository_inserts;
pub mod repository_migrations;
pub mod repository_queries;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("SQL Error: {0}")]
    SQLError(#[from] rusqlite::Error),

    #[error("Error: {0}")]
    GeneralError(String),

    #[error("Migration Error: {0}")]
    MigrationError(String),

    #[error("Error: {0}")]
    InsertionError(String),
}

pub type Result<T> = std::result::Result<T, Error>;

pub struct Repository {
    connection: rusqlite::Connection,
}

impl Repository {
    pub fn connect(path: &std::path::Path) -> Result<Self> {
        Ok(Self {
            connection: rusqlite::Connection::open(path)?,
        })
    }
}

fn get_current_timestamp() -> Result<i64> {
    let timestamp_ms = i64::try_from(
        std::time::SystemTime::now()
            .duration_since(std::time::UNIX_EPOCH)
            .map_err(|_| Error::GeneralError("System time is before UNIX_EPOCH".into()))?
            .as_millis(),
    )
    .map_err(|_| Error::GeneralError("System time millis does not fit into i64".into()))?;
    Ok(timestamp_ms)
}
