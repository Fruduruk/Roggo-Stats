pub mod agent;
pub mod api;
pub mod bl;
pub mod config_watcher;
pub mod db;
pub mod debug;
pub mod logging;
pub mod rl_api;
pub mod agent_supervisor;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("Rocket League api error: {0}")]
    RocketLeagueAPIError(#[from] crate::core::rl_api::Error),

    #[error("Web API error: {0}")]
    WebAPIError(#[from] crate::core::api::Error),

    #[error("Repository Error {0}")]
    RepositoryError(#[from] crate::core::db::Error),

    #[error("Notify Error {0}")]
    NotifyError(#[from] notify::Error),

    #[error("Config Error {0}")]
    ConfigError(String),

    #[error("Shutdown error: {0}")]
    ShutdownError(String),

    #[error("Resume error: {0}")]
    ResumeError(String),
}

pub type Result<T> = std::result::Result<T, Error>;
