use crate::core::{agent::run_agent, logging::init_logging};

pub fn run() -> anyhow::Result<()>{
    let _log_guard = init_logging();
    tracing::info!("Starting roggo agent in console mode");

    let runtime =
        tokio::runtime::Runtime::new().expect("Tokio Runtime konnte nicht gestartet werden");

    runtime.block_on(async {
        run_agent(None).await
    })?;

    Ok(())
}
