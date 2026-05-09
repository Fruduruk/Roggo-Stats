use tokio::sync::watch::Receiver;

use crate::core::{api::Result, db::repository::Repository};

pub struct WebApi {
    repository: Repository,
    shutdown_rx: Receiver<bool>,
}

impl WebApi {
    pub fn new(shutdown_rx: Receiver<bool>) -> Result<Self> {
        Ok(Self {
            repository: Repository::new("test.db")?,
            shutdown_rx,
        })
    }

    pub async fn run(&mut self) -> Result<()> {
        Ok(())
    }
}
