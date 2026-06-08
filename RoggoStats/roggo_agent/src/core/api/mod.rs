use axum::{Json, http, response::IntoResponse};

use crate::core::api::contract::{AgentErrorCode, AgentErrorDto};

pub mod contract;
pub mod mappers;
pub mod web_api;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("User Error: {0}")]
    UserError(String),
    #[error("Internal Error: {0}")]
    InternalError(#[from] crate::core::bl::Error),
    #[error("Axum Error")]
    AxumError { source: std::io::Error },
}

pub type Result<T> = std::result::Result<T, Error>;

impl IntoResponse for Error {
    fn into_response(self) -> axum::response::Response {
        tracing::error!("failed to process web api request: {:#?}", &self);

        let status = match &self {
            Error::UserError(_) => http::StatusCode::BAD_REQUEST,
            Error::InternalError(_error) => http::StatusCode::INTERNAL_SERVER_ERROR,
            Error::AxumError { source: _ } => http::StatusCode::INTERNAL_SERVER_ERROR,
        };

        let body = Json(get_agent_error_dto(self));

        (status, body).into_response()
    }
}

fn get_agent_error_dto(value: Error) -> AgentErrorDto {
    let error_type = match &value {
        Error::InternalError(error) => get_error_code(error),
        Error::AxumError { source: _ } => AgentErrorCode::InternalError,
        Error::UserError(_) => AgentErrorCode::UserError,
    };

    let message = get_error_message(&value);

    AgentErrorDto {
        error: error_type,
        message,
        details: vec![],
    }
}

fn get_error_message(value: &Error) -> String {
    match value {
        Error::InternalError(error) => error.to_string(),
        Error::AxumError { source } => source.to_string(),
        Error::UserError(s) => s.to_string(),
    }
}

fn get_error_code(error: &super::bl::Error) -> AgentErrorCode {
    match error {
        super::bl::Error::RepositoryError(_)
        | super::bl::Error::CalculationError(_)
        | super::bl::Error::InsertionFailed(_) => AgentErrorCode::InternalError,
        super::bl::Error::NoPlayerFound { source: _ } => AgentErrorCode::NoEntries,
    }
}
