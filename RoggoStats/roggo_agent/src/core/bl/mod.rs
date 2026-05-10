pub mod game_stat_collector;
pub mod intermediate_models;


#[derive(thiserror::Error,Debug, Clone)]
pub enum Error {
    #[error("Calculation Error")]
    CalculationError(String),
    #[error("Failed to insert event {0}")]
    InsertionFailed(String),
}

pub type Result<T> = std::result::Result<T, Error>;