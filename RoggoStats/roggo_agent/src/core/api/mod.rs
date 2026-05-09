pub mod web_api;
pub mod dto;

#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("Repository Error {0}")]
    RepositoryError(#[from] crate::core::db::Error),
    #[error("Axum Error {0}")]
    AxumError(#[from] std::io::Error),
}

pub type Result<T> = std::result::Result<T, Error>;