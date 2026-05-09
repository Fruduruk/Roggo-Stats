use std::time::{Duration, SystemTime, UNIX_EPOCH};

use crate::core::rl_api::Result;
use tokio::net::TcpStream;
use tokio::sync::watch;
use tokio::{io::AsyncReadExt, sync::mpsc};

const ROCKET_LEAGUE_TCP_ADDR: &str = "127.0.0.1:49123";

pub async fn read_rocket_league_api(
    tx: mpsc::Sender<(i64, String)>,
    shutdown_rx: watch::Receiver<bool>,
) -> Result<()> {
    loop {
        tracing::debug!("Connecting to Rocket League API...");

        let mut rl_stream = match TcpStream::connect(ROCKET_LEAGUE_TCP_ADDR).await {
            Ok(stream) => stream,
            Err(_) => {
                tracing::debug!("Waiting for Rocket League to start...");
                for i in (1..=10).rev() {
                    tracing::debug!("Retrying in {i}s");
                    tokio::time::sleep(Duration::from_secs(1)).await;
                    if *shutdown_rx.borrow() {
                        tracing::info!("Shutting down rocket league api listener...");
                        return Ok(());
                    }
                }
                continue;
            }
        };

        tracing::info!("Connected with Rocket League API.");

        let mut buffer = [0u8; 8192];

        loop {
            if *shutdown_rx.borrow() {
                tracing::info!("Shutting down rocket league api listener...");
                return Ok(());
            }

            if let Ok(n) = rl_stream.read(&mut buffer).await {
                let timestamp_ms = i64::try_from(
                    SystemTime::now()
                        .duration_since(UNIX_EPOCH)
                        .expect("system time is before UNIX_EPOCH")
                        .as_millis(),
                )
                .expect("What year are you in?");

                if n == 0 {
                    tracing::warn!("Rocket League API connection closed.");
                    break;
                }

                let raw = String::from_utf8_lossy(&buffer[..n]).to_string();

                if let Err(err) = tx.send((timestamp_ms, raw)).await {
                    tracing::error!(error = %err, "Failed to send packet");
                }
            }
        }
    }
}
