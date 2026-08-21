use std::thread;

use crate::core::debug::packet_collector::PacketCollector;
use crate::core::time::{format_ms_to_date_time, now};
use crate::get_dumps_folder_path;

fn dump_raw_bytes(events: Vec<(i64, Vec<u8>)>, path: String) {
    thread::spawn(move || {
        tracing::info!("Saving raw packets to {}...", path);
        let mut collector = PacketCollector::new(path.clone()).expect("couldnt create collector");

        collector.next_bulk(events).expect("collector next failed");

        collector.finish().expect("collector failed to finish");
        tracing::info!("Saved raw packets to {}.", path);
    });
}

pub fn create_error_dump(events: Vec<(i64, Vec<u8>)>, error_message: String) {
    let now = match now() {
        Ok(ms) => ms,
        Err(err) => {
            tracing::error!(error= %err ,"Failed to create dump folder");
            return;
        },
    };
    let dump_directory = get_dumps_folder_path().join(format_ms_to_date_time(now));
    if let Err(err) = std::fs::create_dir_all(&dump_directory) {
        tracing::error!(error = %err, "Failed to create dump directory");
    }

    let dump_file = dump_directory.join("dump.7z");
    let error_file = dump_directory.join("error.txt");

    if let Err(err) = std::fs::write(error_file, error_message) {
        tracing::error!(error = %err, "Failed to write error message");
    }
    dump_raw_bytes(events, dump_file.to_string_lossy().to_string());
}
