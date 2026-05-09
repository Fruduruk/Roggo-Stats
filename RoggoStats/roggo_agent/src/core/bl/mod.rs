pub mod game_stat_collector;
pub mod intermediate_models;


#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("Calculation Error")]
    CalculationError(String),
}

pub type Result<T> = std::result::Result<T, Error>;