use anyhow::{Context, Result};
use tokio::sync::mpsc;
use tokio::sync::watch;

use crate::core::api::web_api::WebApi;
use crate::core::rl_api::aggregator::Aggregator;
use crate::core::rl_api::rocket_league_api::read_rocket_league_api;

pub async fn run_agent(
    shutdown_otpion: Option<(watch::Sender<bool>, watch::Receiver<bool>)>,
) -> Result<()> {
    println!("Roggo agent is running.");

    let (shutdown_tx, shutdown_rx) = match shutdown_otpion {
        Some(watch) => watch,
        None => watch::channel(false),
    };

    let (tx, rx) = mpsc::channel::<(i64, String)>(1_000_000);

    tokio::spawn(async move {
        if tokio::signal::ctrl_c().await.is_ok() {
            println!("CTRL+C Received.");
            let _ = shutdown_tx.send(true);
        }
    });

    let (send_result, receive_result, web_result) = tokio::join!(
        send_packets(shutdown_rx.clone(), tx),
        receive_packets(rx),
        start_web_api(shutdown_rx)
    );

    send_result.context("Packet sender failed.")?;
    receive_result.context("Packet receiver failed.")?;
    web_result.context("Web API task failed.")?;

    Ok(())
}

async fn start_web_api(shutdown_rx: watch::Receiver<bool>) -> Result<()> {
    println!("Web API is running.");
    WebApi::run(shutdown_rx).await.context("Web API failed.")?;
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
        .await.context("Failed to read test files.")?;
    } else {
        read_rocket_league_api(tx, shutdown_rx.clone()).await.context("Faild to read rocket league api.")?;
    }
    Ok(())
}

async fn receive_packets(mut rx: mpsc::Receiver<(i64, String)>) -> Result<()> {
    println!("Aggregator is running.");
    // let mut packet_collector = crate::core::packet_collector::PacketCollector::new("captures/new")?;
    let mut aggregator = Aggregator::new();
    let mut count: u128 = 0;

    while let Some((timestamp, raw)) = rx.recv().await {
        print!("\rReceiving packet number {}", count);
        // packet_collector.next(timestamp, &raw);
        aggregator.insert(timestamp, raw);
        count += 1;
    }

    println!("Packet channel closed. Received {count} packets!");

    Ok(())
}
