use axum::{Json, http, response::IntoResponse};

pub mod dto;
pub mod web_api;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("Repository Error {0}")]
    RepositoryError(#[from] crate::core::db::Error),
    #[error("Axum Error {0}")]
    AxumError(#[from] std::io::Error),
}

pub type Result<T> = std::result::Result<T, Error>;

impl IntoResponse for Error {
    fn into_response(self) -> axum::response::Response {
        tracing::error!("failed to process web api request: {:#?}", &self);

        let status = match self {
            Error::RepositoryError(_) => http::StatusCode::INTERNAL_SERVER_ERROR,
            Error::AxumError(_) => http::StatusCode::INTERNAL_SERVER_ERROR,
        };

        let body = Json(serde_json::json!({
            "error": "Something went wrong."//self.to_string()
        }));

        (status, body).into_response()
    }
}
