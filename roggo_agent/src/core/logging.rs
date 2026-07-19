use tracing_appender::non_blocking::WorkerGuard;
use tracing_subscriber::{fmt, layer::SubscriberExt, util::SubscriberInitExt, EnvFilter};

use crate::get_app_data_directory;

pub fn init_logging() -> WorkerGuard {
    let directory = get_app_data_directory().join("logs");
    let file_appender = tracing_appender::rolling::daily(directory, "roggo-agent.log");
    let (file_writer, guard) = tracing_appender::non_blocking(file_appender);

    let filter = EnvFilter::try_from_default_env()
        .unwrap_or_else(|_| EnvFilter::new("roggo_agent=debug,info"));

    let file_layer = fmt::layer()
        .with_writer(file_writer)
        .with_ansi(false)
        .with_target(true)
        .with_thread_ids(true)
        .with_file(true)
        .with_line_number(true);

    let stdout_layer = fmt::layer()
    .with_writer(std::io::stdout)
    .with_ansi(true)
    .with_target(false)
    .compact();

    tracing_subscriber::registry()
        .with(filter)
        .with(file_layer)
        .with(stdout_layer)
        .init();

    guard
}