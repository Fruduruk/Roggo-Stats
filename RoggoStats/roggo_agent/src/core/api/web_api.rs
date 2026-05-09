use crate::core::api::error::Result;

pub struct WebApi {
    
}
impl WebApi {
    pub async fn run(_shutdown_rx: tokio::sync::watch::Receiver<bool>) -> Result<()> {
        Ok(())
    }
}