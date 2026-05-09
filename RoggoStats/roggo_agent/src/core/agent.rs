use tokio::sync::mpsc;
use tokio::sync::watch;

use crate::core::api::web_api;
use crate::core::rl_api::aggregator::Aggregator;
use crate::core::rl_api::rocket_league_api::read_rocket_league_api;

use crate::core::{Error, Result};

pub async fn run_agent(
    shutdown_otpion: Option<(watch::Sender<bool>, watch::Receiver<bool>)>,
) -> Result<()> {
    tracing::info!("Roggo agent is running");

    let (shutdown_tx, shutdown_rx) = match shutdown_otpion {
        Some(watch) => watch,
        None => watch::channel(false),
    };

    let (tx, rx) = mpsc::channel::<(i64, String)>(1_000_000);

    tokio::spawn(async move {
        if tokio::signal::ctrl_c().await.is_ok() {
            tracing::info!("Shutting down agent...");
            let _ = shutdown_tx.send(true);
        }
    });

    let (send_result, receive_result, web_result) = tokio::join!(
        send_packets(shutdown_rx.clone(), tx),
        receive_packets(shutdown_rx.clone(), rx),
        start_web_api(shutdown_rx)
    );

    let mut errors = Vec::new();

    if let Err(err) = send_result {
        errors.push(err);
    }

    if let Err(err) = receive_result {
        errors.push(err);
    }

    if let Err(err) = web_result {
        errors.push(err);
    }

    if !errors.is_empty() {
        return Err(Error::Multiple(errors));
    }

    Ok(())
}

async fn start_web_api(shutdown_rx: watch::Receiver<bool>) -> Result<()> {
    tracing::info!("Web API is running");
    web_api::run(shutdown_rx).await?;
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
) -> Result<()> {
    tracing::info!("Aggregator is running");
    // let mut packet_collector = crate::core::packet_collector::PacketCollector::new("captures/new")?;
    let mut aggregator = Aggregator::new()?;
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
