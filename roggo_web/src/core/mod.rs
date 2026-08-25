use crate::core::contract::{AgentErrorDto};
pub mod app;
pub mod api;
pub mod ui;
pub mod contract;
pub mod time;
pub mod links;
pub mod tasks;
pub mod app_state;
pub mod api_result;

#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("HTTP error")]
    HTTPError(#[from] gloo_net::Error),

    #[error("Agent Error")]
    AgentError(AgentErrorDto)
}
pub type Result<T> = std::result::Result<T, Error>;
