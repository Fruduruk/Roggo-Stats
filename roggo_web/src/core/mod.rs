use crate::core::contract::AgentErrorDto;
pub mod api;
pub mod api_result;
pub mod app;
pub mod app_state;
pub mod contract;
pub mod links;
pub mod tasks;
pub mod time;
pub mod ui;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("HTTP error")]
    HTTPError(#[from] gloo_net::Error),

    #[error("Agent Error")]
    AgentError(AgentErrorDto),

    #[error("General Error")]
    GeneralError(String),
}
pub type Result<T> = std::result::Result<T, Error>;
