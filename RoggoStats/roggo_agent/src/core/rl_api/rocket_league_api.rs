use std::time::{Duration, SystemTime, UNIX_EPOCH};

use crate::core::rl_api::{Error, Result};
use tokio::io::AsyncReadExt;
use tokio::net::TcpStream;
use tokio::sync::{mpsc, watch};

const ROCKET_LEAGUE_TCP_ADDR: &str = "127.0.0.1";

pub async fn read_rocket_league_api(
    port: u16,
    tx: mpsc::Sender<(i64, String)>,
    shutdown_rx: watch::Receiver<bool>,
) -> Result<()> {
    loop {
        let mut rl_stream = match TcpStream::connect(format!("{ROCKET_LEAGUE_TCP_ADDR}:{port}")).await {
            Ok(stream) => stream,
            Err(_) => {
                tokio::select! {
                    _ = wait_for_shutdown(shutdown_rx.clone()) => {
                        tracing::info!("Shutting down rocket league api listener...");
                        return Ok(());
                    }

                    _ = tokio::time::sleep(Duration::from_secs(10)) => {}
                }
                continue;
            }
        };

        tracing::info!("Connected with Rocket League API.");

        tokio::select! {
            _ = wait_for_shutdown(shutdown_rx.clone()) => {
                tracing::info!("Shutting down rocket league api listener...");
                return Ok(());
            }

            result = read_tcp_packets(&mut rl_stream,&tx) => {
                match result {
                    Ok(()) => {
                        tracing::warn!("Rocket League API disconnected. Reconnecting...");
                        continue;
                    }

                    Err(err) => {
                        return Err(err);
                    }
                }
            }
        }
    }
}

async fn wait_for_shutdown(mut shutdown_rx: watch::Receiver<bool>) {
    loop {
        if *shutdown_rx.borrow() {
            return;
        }

        if shutdown_rx.changed().await.is_err() {
            return;
        }
    }
}

async fn read_tcp_packets(
    rl_stream: &mut TcpStream,
    tx: &mpsc::Sender<(i64, String)>,
) -> Result<()> {
    let mut buffer = [0u8; 8192];

    loop {
        match read_tcp_packet(rl_stream, &mut buffer, tx).await {
            Ok(()) => {}

            Err(Error::APIConnectionClosed) => {
                return Ok(());
            }

            Err(err) => {
                return Err(err);
            }
        }
    }
}

async fn read_tcp_packet(
    rl_stream: &mut TcpStream,
    buffer: &mut [u8; 8192],
    tx: &mpsc::Sender<(i64, String)>,
) -> Result<()> {
    let n = rl_stream.read(buffer).await.unwrap_or(0);

    let timestamp_ms = i64::try_from(
        SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .map_err(|_| Error::GeneralError("System time is before UNIX_EPOCH".into()))?
            .as_millis(),
    )
    .map_err(|_| Error::GeneralError("System time millis does not fit into i64".into()))?;

    if n == 0 {
        tracing::warn!("Rocket League API connection closed");
        return Err(Error::APIConnectionClosed);
    }

    let raw = String::from_utf8_lossy(&buffer[..n]).to_string();

    if let Err(err) = tx.send((timestamp_ms, raw)).await {
        tracing::error!(error = %err, "Failed to send packet");
    }

    Ok(())
}
