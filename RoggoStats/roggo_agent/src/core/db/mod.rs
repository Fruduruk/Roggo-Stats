pub mod repository_inserts;
pub mod repository_queries;


const SCHEMA_VERSION: i64 = 1;
const AGENT_VERSION: &str = "0.1.0";

#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("SQL Error: {0}")]
    SQLError(#[from] rusqlite::Error),
    
    #[error("Error: {0}")]
    GeneralError(String),
}

pub type Result<T> = std::result::Result<T, Error>;


pub struct Repository {
    connection: rusqlite::Connection,
}

impl Repository {
    pub fn new(path: &std::path::Path) -> Result<Self> {
        let repo = Self::connect(path)?;
        repo.init()?;
        Ok(repo)
    }

    pub fn connect(path: &std::path::Path) -> Result<Self> {
        Ok(Self {
            connection: rusqlite::Connection::open(path)?,
        })
    }

    pub fn new_in_memory() -> Result<Self> {
        let repo = Self {
            connection: rusqlite::Connection::open_in_memory()?,
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

}