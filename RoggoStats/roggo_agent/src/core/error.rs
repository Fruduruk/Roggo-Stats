#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("failed to create aggregator")]
    AggregatorCreationFailed {
        #[source]
        source: crate::core::rl_api::error::Error,
    },
    #[error("failed to read rocket league api")]
    RocketLeagueApiReadingFailed {
        #[source]
        source: crate::core::rl_api::error::Error,
    },

    #[error("Web API failed")]
    WebAPIError(#[from] crate::core::api::error::Error),
}

pub type Result<T> = std::result::Result<T, Error>;
