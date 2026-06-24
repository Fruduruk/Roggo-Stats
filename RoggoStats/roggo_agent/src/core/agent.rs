use std::path::PathBuf;

use tokio::sync::mpsc;
use tokio::sync::watch;
use tokio::task::JoinError;

use crate::core::api::web_api;
use crate::core::rl_api::aggregator::Aggregator;
use crate::core::rl_api::rocket_league_api::read_rocket_league_api;
use crate::core::{Error, Result};
use crate::settings::models::AgentConfig;

enum FinishedTask {
    Sender(std::result::Result<Result<()>, JoinError>),
    Receiver(std::result::Result<Result<()>, JoinError>),
    Web(std::result::Result<Result<()>, JoinError>),
}

pub async fn run_agent_instance(
    shutdown_tx: watch::Sender<bool>,
    shutdown_rx: watch::Receiver<bool>,
    config: AgentConfig,
    db_file_path: PathBuf,
) -> Result<()> {
    let final_shutdown_tx = shutdown_tx.clone();

    let (tx, rx) = mpsc::channel::<(i64, String)>(1_000_000);

    let mut send_handle = tokio::spawn(send_packets(config, shutdown_rx.clone(), tx));

    let mut receive_handle = tokio::spawn(receive_packets(
        shutdown_rx.clone(),
        rx,
        db_file_path.clone(),
    ));

    let mut web_handle = tokio::spawn(start_web_api(shutdown_rx.clone(), db_file_path));

    let finished_task = tokio::select! {
        result = &mut send_handle => {
            FinishedTask::Sender(result)
        }

        result = &mut receive_handle => {
            FinishedTask::Receiver(result)
        }

        result = &mut web_handle => {
            FinishedTask::Web(result)
        }
    };

    let _ = final_shutdown_tx.send(true);

    let finished_first_result = match finished_task {
        FinishedTask::Sender(result) => {
            let _ = web_handle.await;
            let _ = receive_handle.await;
            result
        }

        FinishedTask::Receiver(result) => {
            let _ = web_handle.await;
            let _ = send_handle.await;
            result
        }

        FinishedTask::Web(result) => {
            let _ = send_handle.await;
            let _ = receive_handle.await;
            result
        }
    };

    finished_first_result.map_err(|err| Error::ShutdownError(err.to_string()))?
}

async fn start_web_api(shutdown_rx: watch::Receiver<bool>, db_file_path: PathBuf) -> Result<()> {
    tracing::info!("Web API is running");

    web_api::run(shutdown_rx, db_file_path).await?;

    Ok(())
}

async fn send_packets(
    config: AgentConfig,
    shutdown_rx: watch::Receiver<bool>,
    tx: mpsc::Sender<(i64, String)>,
) -> Result<()> {
    if let Ok(path) = std::env::var("import_path") {
        crate::core::debug::test_file_reader::read_test_files_from_7z(
            path,
            tx,
            shutdown_rx.clone(),
        )
        .await;
    } else {
        read_rocket_league_api(config, tx, shutdown_rx.clone()).await?;
    }

    Ok(())
}

async fn receive_packets(
    shutdown_rx: watch::Receiver<bool>,
    mut rx: mpsc::Receiver<(i64, String)>,
    db_file_path: PathBuf,
) -> Result<()> {
    tracing::info!("Aggregator is running");
    // let mut packet_collector =
    //     crate::core::debug::packet_collector::PacketCollector::new("captures/new.7z").unwrap();
    let mut aggregator = Aggregator::new(db_file_path);
    let mut count: u128 = 0;

    while let Some((timestamp, raw)) = rx.recv().await {
        if *shutdown_rx.borrow() {
            return Ok(());
        }

        print!("\rReceiving packet number {}", count);
        // packet_collector.next(timestamp, &raw).map_err(|err| Error::ShutdownError(err.to_string()))?;
        if let Err(err) = aggregator.insert(timestamp, raw) {
            tracing::warn!(error=%err,"failed to insert packet");
        }
        count += 1;
    }

    // packet_collector.finish().map_err(|err| Error::ShutdownError(err.to_string()))?;

    tracing::info!("Shutting down aggregator...");
    Ok(())
}
