mod core;
mod runtimes;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum RunMode {
    Console,
    Tray,
}

fn main() {
    let mode = parse_run_mode();

    match mode {
        RunMode::Console => runtimes::console::run(),
        RunMode::Tray => runtimes::tray::run(),
    }
}

fn parse_run_mode() -> RunMode {
    let args: Vec<String> = std::env::args().collect();

    if args.iter().any(|arg| arg == "--console" || arg == "--debug") {
        return RunMode::Console;
    }

    RunMode::Tray
}