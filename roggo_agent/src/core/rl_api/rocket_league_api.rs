use std::time::{Duration, SystemTime, UNIX_EPOCH};

use crate::WEB_UI_URL;
use crate::core::rl_api::{Error, Result};
use crate::settings::models::AgentConfig;
use tokio::io::AsyncReadExt;
use tokio::net::TcpStream;
use tokio::sync::{mpsc, watch};

const ROCKET_LEAGUE_TCP_ADDR: &str = "127.0.0.1";

pub async fn read_rocket_league_api(
    config: AgentConfig,
    tx: mpsc::Sender<(i64, Vec<u8>)>,
    shutdown_rx: watch::Receiver<bool>,
) -> Result<()> {
    loop {
        let mut rl_stream =
            match TcpStream::connect(format!("{ROCKET_LEAGUE_TCP_ADDR}:{}", config.rl_api_port))
                .await
            {
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
                        if config.start_ui_when_rl_closes {
                            let _ = open::that(WEB_UI_URL);
                        }

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
    tx: &mpsc::Sender<(i64, Vec<u8>)>,
) -> Result<()> {
    let mut buffer = [0u8; 8192];

    loop {
        match read_tcp_segment(rl_stream, &mut buffer).await {
            Ok((timestamp, bytes)) => {
                if let Err(err) = tx.send((timestamp, bytes)).await {
                    tracing::error!(error = %err, "Failed to send bytes");
                }
            }

            Err(Error::APIConnectionClosed) => {
                return Ok(());
            }

            Err(err) => {
                return Err(err);
            }
        }
    }
}

async fn read_tcp_segment(
    rl_stream: &mut TcpStream,
    buffer: &mut [u8; 8192],
) -> Result<(i64, Vec<u8>)> {
    let n = rl_stream.read(buffer).await.unwrap_or_default();

    let timestamp_ms = now()?;

    if n == 0 {
        tracing::warn!("Rocket League API connection closed");
        return Err(Error::APIConnectionClosed);
    }

    let raw = buffer[..n].to_vec();

    Ok((timestamp_ms, raw))
}

#[inline]
fn now() -> Result<i64> {
    i64::try_from(
        SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .map_err(|_| Error::GeneralError("System time is before UNIX_EPOCH".into()))?
            .as_millis(),
    )
    .map_err(|_| Error::GeneralError("System time millis does not fit into i64".into()))
}
