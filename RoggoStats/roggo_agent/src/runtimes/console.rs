use crate::core::agent::RoggoAgent;
use tokio::sync::watch;

pub fn run() {
    println!("Starting roggo agent in console mode");
    let (shutdown_tx, shutdown_rx) = watch::channel(false);

    let runtime =
        tokio::runtime::Runtime::new().expect("Tokio Runtime konnte nicht gestartet werden");

    runtime.block_on(async {
        let mut agent = RoggoAgent::new(shutdown_rx);

        tokio::select! {
            result = agent.run() => {
                if let Err(err) = result {
                    eprintln!("Agent Fehler: {err}");
                }
            }

            _ = tokio::signal::ctrl_c() => {
                println!("Ctrl+C empfangen. Beende Agent.");
                let _ = shutdown_tx.send(true);
            }
        }
    });
}
