use crate::core::{agent::run_agent, logging::init_logging};

pub fn run() {
    let _log_guard = init_logging();

    tracing::info!("Starting roggo agent in console mode");

    let runtime = match tokio::runtime::Runtime::new() {
        Ok(runtime) => runtime,
        Err(err) => {
            tracing::error!(error = %err, "Failed to create Tokio runtime");
            return;
        }
    };

    if let Err(err) = runtime.block_on(run_agent(None)) {
        tracing::error!(error = %err, "Roggo agent failed");
    }
}