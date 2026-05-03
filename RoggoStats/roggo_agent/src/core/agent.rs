use std::error::Error;

use futures_util::{SinkExt, StreamExt};
use tokio::io::AsyncReadExt;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::{broadcast, watch};
use tokio_tungstenite::tungstenite::Message;

use crate::core::aggregator;

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

        let (live_tx, _) = broadcast::channel::<String>(256);

        let receiver_task = tokio::spawn(read_rocket_league_api(
            live_tx.clone(),
            self.shutdown_rx.clone(),
        ));

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

async fn read_rocket_league_api(
    live_tx: broadcast::Sender<String>,
    mut shutdown_rx: watch::Receiver<bool>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    println!("Connecting to Rocket League API...");

    let mut rl_stream = TcpStream::connect(ROCKET_LEAGUE_TCP_ADDR).await?;

    println!("Connected with Rocket League API.");

    let mut buffer = [0u8; 8192];

    loop {
        tokio::select! {
            _ = shutdown_rx.changed() => {
                if *shutdown_rx.borrow() {
                    println!("Rocket-League-Reader closed.");
                    break;
                }
            }

            result = rl_stream.read(&mut buffer) => {
                let n = result?;

                if n == 0 {
                    println!("Rocket League API connection closed.");
                    break;
                }

                let raw = String::from_utf8_lossy(&buffer[..n]).to_string();

                let _ = live_tx.send(raw);
            }
        }
    }

    Ok(())
}

async fn run_websocket_server(
    live_tx: broadcast::Sender<String>,
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
    mut live_rx: broadcast::Receiver<String>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    println!("WebSocket client connected.");

    let ws_stream = tokio_tungstenite::accept_async(client_stream).await?;
    let (mut ws_write, _) = ws_stream.split();

    loop {
        let raw = live_rx.recv().await?;

        ws_write.send(Message::Text(raw.into())).await?;
    }
}

async fn run_aggregator(
    live_tx: broadcast::Sender<String>,
    mut shutdown_rx: watch::Receiver<bool>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    println!("Aggregator is running.");

    let mut live_rx = live_tx.subscribe();

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
                    Ok(raw) => {
                        aggregator::aggregate(raw);
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