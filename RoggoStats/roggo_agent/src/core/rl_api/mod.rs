pub mod aggregator;
pub mod deserializer;
pub mod models;
pub mod rocket_league_api;

#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("Repository Error {0}")]
    RepositoryError(#[from] crate::core::db::Error),
    #[error("Failed to create aggregator")]
    AggregatorCreationFailed {
        #[source]
        source: crate::core::db::Error,
    },
}

pub type Result<T> = std::result::Result<T, Error>;
