use std::error::Error;
use tokio::sync::watch;
use tokio::time::{Duration, sleep};

pub struct RoggoAgent {
    shutdown_rx: watch::Receiver<bool>,
}

impl RoggoAgent {
    pub fn new(shutdown_rx: watch::Receiver<bool>) -> Self {
        Self { shutdown_rx }
    }

    pub async fn run(&mut self) -> Result<(), Box<dyn Error>> {
        println!("RoggoAgent Core läuft.");

        loop {
            println!("Agent Tick...");
            sleep(Duration::from_secs(1)).await;
        }
    }
}
