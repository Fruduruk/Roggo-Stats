use crate::core::dto::{AgentErrorDto};

pub mod api;
pub mod ui;
pub mod dto;

#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("HTTP error")]
    HTTPError(#[from] gloo_net::Error),

    #[error("Agent Error")]
    AgentError(AgentErrorDto)
}
pub type Result<T> = std::result::Result<T, Error>;
