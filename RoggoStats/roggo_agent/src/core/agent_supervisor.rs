use std::path::PathBuf;

use tokio::sync::watch;
use tokio::task::JoinError;

use crate::core::agent::run_agent_instance;
use crate::core::config_watcher::wait_until_changed;
use crate::core::db::Repository;
use crate::core::{Error, Result};
use crate::get_config_file_path;
use crate::load_config_or_default;

enum FinishedTask {
    Agent(std::result::Result<Result<()>, JoinError>),
    ConfigWatcher(std::result::Result<Result<()>, JoinError>),
    ShutdownRequested,
}

pub async fn run_agent(
    shutdown_option: Option<(watch::Sender<bool>, watch::Receiver<bool>)>,
    db_file_path: PathBuf,
) -> Result<()> {
    tracing::info!("Roggo agent supervisor is running");

    tracing::info!("Creating database if needed");
    Repository::new(&db_file_path)?;

    let (global_shutdown_tx, mut global_shutdown_rx) = match shutdown_option {
        Some(channel) => channel,
        None => watch::channel(false),
    };

    let ctrl_c_shutdown_tx = global_shutdown_tx.clone();

    tokio::spawn(async move {
        if tokio::signal::ctrl_c().await.is_ok() {
            tracing::info!("Shutting down agent...");
            let _ = ctrl_c_shutdown_tx.send(true);
        }
    });

    loop {
        let config = load_config_or_default();

        tracing::debug!("{:#?}", config);

        let (instance_shutdown_tx, instance_shutdown_rx) = watch::channel(false);

        let mut agent_handle = tokio::spawn(run_agent_instance(
            instance_shutdown_tx.clone(),
            instance_shutdown_rx,
            config,
            db_file_path.clone(),
        ));

        let mut config_watcher_handle = tokio::spawn(wait_until_changed(get_config_file_path()));

        let finished_task = tokio::select! {
            result = &mut agent_handle => {
                FinishedTask::Agent(result)
            }

            result = &mut config_watcher_handle => {
                FinishedTask::ConfigWatcher(result)
            }

            changed = global_shutdown_rx.changed() => {
                match changed {
                    Ok(()) if *global_shutdown_rx.borrow() => {
                        FinishedTask::ShutdownRequested
                    }

                    Ok(()) => {
                        continue;
                    }

                    Err(_) => {
                        FinishedTask::ShutdownRequested
                    }
                }
            }
        };

        match finished_task {
            FinishedTask::Agent(result) => {
                config_watcher_handle.abort();

                return result.map_err(join_error)?;
            }

            FinishedTask::ConfigWatcher(result) => {
                let config_result = result.map_err(join_error)?;

                if let Err(err) = config_result {
                    tracing::error!(error = %err, "Config watcher failed");

                    let _ = instance_shutdown_tx.send(true);
                    let _ = agent_handle.await;

                    return Err(err);
                }

                tracing::info!("Config changed. Restarting agent instance...");

                let _ = instance_shutdown_tx.send(true);

                agent_handle.await.map_err(join_error)??;

                tracing::info!("Agent instance stopped. Reloading config...");
            }

            FinishedTask::ShutdownRequested => {
                tracing::info!("Shutdown requested. Stopping agent instance...");

                config_watcher_handle.abort();

                let _ = instance_shutdown_tx.send(true);

                return agent_handle.await.map_err(join_error)?;
            }
        }
    }
}

fn join_error(err: JoinError) -> Error {
    Error::ShutdownError(err.to_string())
}
