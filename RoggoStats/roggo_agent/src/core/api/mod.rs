use axum::{Json, http, response::IntoResponse};

pub mod dto;
pub mod web_api;
pub mod mappers;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("Axum Error {0}")]
    InternalError(#[from] crate::core::bl::Error),
    #[error("Axum Error")]
    AxumError{
        source: std::io::Error
    },
}

pub type Result<T> = std::result::Result<T, Error>;

impl IntoResponse for Error {
    fn into_response(self) -> axum::response::Response {
        tracing::error!("failed to process web api request: {:#?}", &self);

        let status = match self {
            Error::InternalError(_) => http::StatusCode::INTERNAL_SERVER_ERROR,
            Error::AxumError{source: _} => http::StatusCode::INTERNAL_SERVER_ERROR,
        };

        let body = Json(serde_json::json!({
            "error": self.to_string() //"Something went wrong."
        }));

        (status, body).into_response()
    }
}
