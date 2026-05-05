#![cfg_attr(target_os = "windows", windows_subsystem = "windows")]

use roggo_agent::runtimes::tray;

fn main() {
    tray::run();
}