pub mod repository;

#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("SQL Error: {0}")]
    SQLError(#[from] rusqlite::Error),
}

pub type Result<T> = std::result::Result<T, Error>;