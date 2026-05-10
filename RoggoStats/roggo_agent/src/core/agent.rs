use std::path::PathBuf;

use tokio::sync::mpsc;
use tokio::sync::watch;
use tokio::task::JoinError;

use crate::core::api::web_api;
use crate::core::db::repository::Repository;
use crate::core::rl_api::aggregator::Aggregator;
use crate::core::rl_api::rocket_league_api::read_rocket_league_api;

use crate::core::{Error, Result};

enum FinishedTask {
    Sender(std::result::Result<Result<()>, JoinError>),
    Receiver(std::result::Result<Result<()>, JoinError>),
    Web(std::result::Result<Result<()>, JoinError>),
}

pub async fn run_agent(
    shutdown_otpion: Option<(watch::Sender<bool>, watch::Receiver<bool>)>,
    db_file_path: PathBuf,
) -> Result<()> {
    tracing::info!("Roggo agent is running");

    tracing::info!("Creating database if needed");
    Repository::new(&db_file_path)?;

    let (shutdown_tx, shutdown_rx) = match shutdown_otpion {
        Some(watch) => watch,
        None => watch::channel(false),
    };

    let final_shutdown_tx = shutdown_tx.clone();

    let (tx, rx) = mpsc::channel::<(i64, String)>(1_000_000);

    tokio::spawn(async move {
        if tokio::signal::ctrl_c().await.is_ok() {
            tracing::info!("Shutting down agent...");
            let _ = shutdown_tx.send(true);
        }
    });

    let mut send_handle = tokio::spawn(send_packets(shutdown_rx.clone(), tx));
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

    final_shutdown_tx
        .send(true)
        .map_err(|err| Error::ShutdownError(err.to_string()))?;

    let finished_first_result = match finished_task {
        FinishedTask::Sender(result) => {
            _ = web_handle.await;
            _ = receive_handle.await;
            result
        }
        FinishedTask::Receiver(result) => {
            _ = web_handle.await;
            _ = send_handle.await;
            result
        }
        FinishedTask::Web(result) => {
            _ = send_handle.await;
            _ = receive_handle.await;
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
        read_rocket_league_api(tx, shutdown_rx.clone()).await?;
    }
    Ok(())
}

async fn receive_packets(
    shutdown_rx: watch::Receiver<bool>,
    mut rx: mpsc::Receiver<(i64, String)>,
    db_file_path: PathBuf,
) -> Result<()> {
    tracing::info!("Aggregator is running");
    // let mut packet_collector = crate::core::packet_collector::PacketCollector::new("captures/new")?;
    let mut aggregator = Aggregator::new(db_file_path);
    let mut count: u128 = 0;

    while let Some((timestamp, raw)) = rx.recv().await {
        if *shutdown_rx.borrow() {
            return Ok(());
        }

        print!("\rReceiving packet number {}", count);
        // packet_collector.next(timestamp, &raw);
        if let Err(err) = aggregator.insert(timestamp, raw) {
            tracing::warn!(error=%err,"failed to insert packet");
        }
        count += 1;
    }

    tracing::info!("Shutting down aggregator...");

    Ok(())
}
