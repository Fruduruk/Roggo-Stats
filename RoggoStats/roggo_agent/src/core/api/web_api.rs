use anyhow::{Context, Result};
pub struct WebApi {
    
}
impl WebApi {
    pub async fn run(shutdown_rx: tokio::sync::watch::Receiver<bool>) -> Result<()> {
        Ok(())
    }
}