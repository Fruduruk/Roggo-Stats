use crate::core::agent::run_agent;

pub fn run() {
    println!("Starting roggo agent in console mode");

    let runtime =
        tokio::runtime::Runtime::new().expect("Tokio Runtime konnte nicht gestartet werden");

    runtime.block_on(async {
        if let Err(err) = run_agent(None).await {
            eprintln!("Agent error: {err}");
        };
    });
}
