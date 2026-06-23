use crate::core::{agent_supervisor::run_agent, logging::init_logging};

const DB_FILE_PATH: &str = "test.db";

pub fn run() {
    let _log_guard = init_logging();

    tracing::info!("Starting roggo agent in console mode");

    let instance = single_instance::SingleInstance::new("roggo-stats-agent")
        .expect("Failed to create single instance lock");

    if !instance.is_single() {
        tracing::info!("Roggo Stats Agent is already running. Exiting.");
        return;
    }


    let runtime = match tokio::runtime::Runtime::new() {
        Ok(runtime) => runtime,
        Err(err) => {
            tracing::error!(error = %err, "Failed to create Tokio runtime");
            return;
        }
    };

    if let Err(err) = runtime.block_on(run_agent(None, DB_FILE_PATH.into())) {
        tracing::error!(error = %err, "Roggo agent failed");
    }

    tracing::info!("Roggo Agent shut down gracefully.");
}
