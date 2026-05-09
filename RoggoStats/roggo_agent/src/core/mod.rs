
pub mod agent;
pub mod api;
pub mod bl;
pub mod db;
pub mod debug;
pub mod rl_api;
pub mod logging;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("Rocket League api error: {0}")]
    RocketLeagueAPIError(#[from] crate::core::rl_api::Error),

    #[error("Web API error: {0}")]
    WebAPIError(#[from] crate::core::api::Error),

    #[error("Shutdown error: {0}")]
    ShutdownError(String),
}

pub type Result<T> = std::result::Result<T, Error>;
