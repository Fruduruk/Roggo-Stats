use std::error::Error;

use crate::core::{
    aggregator::Aggregator, rocket_league_api::read_rocket_league_api,
    test_file_reader::read_test_files,
};
use tokio::sync::mpsc;
use tokio::sync::watch;

pub async fn run_agent(
    shutdown_otpion: Option<(watch::Sender<bool>, watch::Receiver<bool>)>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    println!("Roggo agent is running.");

    let (shutdown_tx, shutdown_rx) = match shutdown_otpion {
        Some(watch) => watch,
        None => watch::channel(false),
    };

    let (tx, rx) = mpsc::channel::<(i64, String)>(1_000_000);

    let _cancel_task = tokio::spawn(async move {
        if tokio::signal::ctrl_c().await.is_ok() {
            println!("CTRL+C Received.");
            let _ = shutdown_tx.send(true);
        }
    });

    let sender_task = if let Ok(path) = std::env::var("import_path") {
        tokio::spawn(read_test_files(path, tx, shutdown_rx.clone()))
    } else {
        tokio::spawn(read_rocket_league_api(tx, shutdown_rx.clone()))
    };

    let receiver_task = tokio::spawn(receive_packets(rx));

    let _ = sender_task.await?;
    let _ = receiver_task.await?;

    Ok(())
}

async fn receive_packets(
    mut rx: mpsc::Receiver<(i64, String)>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
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
