pub mod agent;
pub mod api;
pub mod bl;
pub mod db;
pub mod debug;
pub mod rl_api;
pub mod logging;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("rocket league api error: {0}")]
    RocketLeagueAPIError(#[from] crate::core::rl_api::Error),

    #[error("web api error: {0}")]
    WebAPIError(#[from] crate::core::api::Error),

    #[error("multiple agent tasks failed: {0:?}")]
    Multiple(Vec<Error>),
}

pub type Result<T> = std::result::Result<T, Error>;
