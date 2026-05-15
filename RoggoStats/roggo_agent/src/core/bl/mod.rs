pub mod game_stat_collector;
pub mod intermediate_models;
pub mod feature;
pub mod query_models;

#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("Repository Error {0}")]
    RepositoryError(#[from] crate::core::db::Error),
    #[error("Calculation Error")]
    CalculationError(String),
    #[error("Failed to insert event {0}")]
    InsertionFailed(String),
    #[error("No player found")]
    NoPlayerFound{
        source: crate::core::db::Error
    },
}
pub type Result<T> = std::result::Result<T, Error>;
