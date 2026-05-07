use std::error::Error;

use std::{
    fs,
    path::{Path, PathBuf},
};
use tokio::sync::{mpsc, watch};

pub async fn read_test_files(
    dir: impl AsRef<Path>,
    tx: mpsc::Sender<(i64, String)>,
    shutdown_rx: watch::Receiver<bool>,
) -> Result<(), Box<dyn Error + Send + Sync>> {
    let mut files: Vec<PathBuf> = fs::read_dir(dir)?
        .filter_map(|entry| entry.ok())
        .map(|entry| entry.path())
        .filter(|path| path.extension().is_some_and(|ext| ext == "json"))
        .collect();

    files.sort();
    println!("Sending {} packets...", files.len());
    for file in files {
        if *shutdown_rx.borrow() {
            break;
        }

        // tokio::time::sleep(std::time::Duration::from_millis(7)).await; // 130 inputs per second

        let raw = fs::read_to_string(&file)?;

        let file_name_timestamp = file
            .file_stem()
            .expect("No file name")
            .to_str()
            .expect("This is not a string?")
            .parse()
            .expect("Not a valid i64");


        // println!("Sending {}", file.to_str().unwrap());
        if let Err(err) = tx.send((file_name_timestamp, raw)).await {
            println!("Failed to send {}", err);
            break;
        }
    }
    Ok(())
}
