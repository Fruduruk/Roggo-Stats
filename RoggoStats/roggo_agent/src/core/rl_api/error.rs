#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("Cannot create sqlite repository")]
    RepositoryCreationFailed(#[from] crate::core::db::error::Error)
}

pub type Result<T> = std::result::Result<T, Error>;