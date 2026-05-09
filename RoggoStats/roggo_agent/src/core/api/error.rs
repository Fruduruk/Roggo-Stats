#[derive(thiserror::Error,Debug)]
pub enum Error {
    #[error("Web API Error")]
    E
}

pub type Result<T> = std::result::Result<T, Error>;