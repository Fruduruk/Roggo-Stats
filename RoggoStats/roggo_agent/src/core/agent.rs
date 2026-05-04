use std::{
    error::Error,
    time::{Duration, SystemTime, UNIX_EPOCH},
};

use crate::core::aggregator::Aggregator;
use futures_util::{SinkExt, StreamExt};
use std::{
    fs,
    path::{Path, PathBuf},
};
use tokio::io::AsyncReadExt;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::{broadcast, watch};
use tokio_tungstenite::tungstenite::Message;

pub struct RoggoAgent {
    shutdown_rx: watch::Receiver<bool>,
}

const ROCKET_LEAGUE_TCP_ADDR: &str = "127.0.0.1:49123";
const LOCAL_WS_ADDR: &str = "127.0.0.1:49124";

impl RoggoAgent {
    pub fn new(shutdown_rx: watch::Receiver<bool>) -> Self {
        Self { shutdown_rx }
    }

    pub async fn run(&mut self) -> Result<(), Box<dyn Error + Send + Sync>> {
        println!("Roggo agent is running.");

        let (live_tx, _) = broadcast::channel::<(u128, String)>(256);

        let import_path = std::env::var("import_path");

        let receiver_task = if let Ok(path) = import_path {
            println!("Starting import in...");
            for i in (0..=1).rev() {
                tokio::time::sleep(Duration::from_secs(1)).await;
                println!("{i}s");
            }
            tokio::spawn(read_test_files(path, live_tx.clone()))
        } else {
            tokio::spawn(read_rocket_league_api(
                live_tx.clone(),
                self.shutdown_rx.clone(),
            ))
        };

        let web_socket_sender_task = tokio::spawn(run_websocket_server(
            live_tx.clone(),
            self.shutdown_rx.clone(),
        ));

        let aggregator_task = tokio::spawn(run_aggregator(live_tx, self.shutdown_rx.clone()));

        tokio::select! {
            result = receiver_task => {
                result??;
            }

            result = web_socket_sender_task => {
                result??;
            }

            result = aggregator_task => {
                result??;
            }

            _ = self.shutdown_rx.changed() => {
                println!("Shutdown received.");
            }
        }

        Ok(())
    }
}

async fn read_test_files(
    dir: impl AsRef<Path>,
    live_tx: broadcast::Sender<(u128, String)>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    let mut files: Vec<PathBuf> = fs::read_dir(dir)?
        .filter_map(|entry| entry.ok())
        .map(|entry| entry.path())
        .filter(|path| path.extension().is_some_and(|ext| ext == "json"))
        .collect();

    files.sort();
    for file in files {
        tokio::time::sleep(std::time::Duration::from_millis(7)).await; // 130 inputs per second
        // println!("{}", file.to_str().unwrap());
        let raw = fs::read_to_string(&file)?;

        let file_name_timestamp = file
            .file_stem()
            .expect("No file name")
            .to_str()
            .expect("This is not a string?")
            .parse()
            .expect("Not a valid u128");

        let _ = live_tx.send((file_name_timestamp, raw));
    }

    Ok(())
}

async fn read_rocket_league_api(
    live_tx: broadcast::Sender<(u128, String)>,
    mut shutdown_rx: watch::Receiver<bool>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    loop {
        println!("Connecting to Rocket League API...");

        let mut rl_stream = match TcpStream::connect(ROCKET_LEAGUE_TCP_ADDR).await {
            Ok(stream) => stream,
            Err(_) => {
                println!("Waiting for Rocket League to start...");
                for i in (1..=10).rev() {
                    println!("Retrying in {i}s");
                    tokio::time::sleep(Duration::from_secs(1)).await;
                }
                continue;
            }
        };

        println!("Connected with Rocket League API.");

        let mut buffer = [0u8; 8192];

        loop {
            tokio::select! {
                _ = shutdown_rx.changed() => {
                    if *shutdown_rx.borrow() {
                        println!("Rocket-League-Reader closed.");
                        return Ok(());
                    }
                }

                result = rl_stream.read(&mut buffer) => {
                    let timestamp_ms = SystemTime::now().duration_since(UNIX_EPOCH).expect("system time is before UNIX_EPOCH").as_millis();
                    let n = result.unwrap_or(0);

                    if n == 0 {
                        println!("Rocket League API connection closed.");
                        break;
                    }

                    let raw = String::from_utf8_lossy(&buffer[..n]).to_string();

                    let _ = live_tx.send((timestamp_ms,raw));
                }
            }
        }
    }
}

async fn run_websocket_server(
    live_tx: broadcast::Sender<(u128, String)>,
    mut shutdown_rx: watch::Receiver<bool>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    let listener = TcpListener::bind(LOCAL_WS_ADDR).await?;

    println!("WebSocket server is running on ws://{LOCAL_WS_ADDR}");

    loop {
        tokio::select! {
            _ = shutdown_rx.changed() => {
                if *shutdown_rx.borrow() {
                    println!("WebSocket server closed.");
                    break;
                }
            }

            accepted = listener.accept() => {
                let (client_stream, _) = accepted?;
                let live_rx = live_tx.subscribe();

                tokio::spawn(async move {
                    if let Err(err) = handle_websocket_client(client_stream, live_rx).await {
                        eprintln!("WebSocket client error: {err}");
                    }
                });
            }
        }
    }

    Ok(())
}

async fn handle_websocket_client(
    client_stream: TcpStream,
    mut live_rx: broadcast::Receiver<(u128, String)>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    println!("WebSocket client connected.");

    let ws_stream = tokio_tungstenite::accept_async(client_stream).await?;
    let (mut ws_write, _) = ws_stream.split();

    loop {
        let (_timestamp, raw) = live_rx.recv().await?;

        ws_write.send(Message::Text(raw.into())).await?;
    }
}

async fn run_aggregator(
    live_tx: broadcast::Sender<(u128, String)>,
    mut shutdown_rx: watch::Receiver<bool>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    println!("Aggregator is running.");

    let mut live_rx = live_tx.subscribe();

    let mut aggregator = Aggregator::new();
    // let mut packet_collector = PacketCollector::new("captures/new")?;

    loop {
        tokio::select! {
            _ = shutdown_rx.changed() => {
                if *shutdown_rx.borrow() {
                    println!("Aggregator closed.");
                    break;
                }
            }

            result = live_rx.recv() => {
                match result {
                    Ok((timestamp,raw)) => {
                        // packet_collector.next(timestamp,&raw);
                        aggregator.insert(timestamp,raw);
                    }

                    Err(broadcast::error::RecvError::Lagged(count)) => {
                        eprintln!("Aggregator lagged and missed {count} messages.");
                    }

                    Err(broadcast::error::RecvError::Closed) => {
                        println!("Broadcast channel closed.");
                        break;
                    }
                }
            }
        }
    }

    Ok(())
}
