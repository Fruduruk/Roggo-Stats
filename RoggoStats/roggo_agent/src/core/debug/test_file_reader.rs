use anyhow::{Context,Result};
use std::ffi::OsStr;
use std::fs::File;
use std::{
    fs,
    path::{Path, PathBuf},
};
use tokio::sync::{mpsc, watch};

use sevenz_rust::{Password, SevenZReader};

pub async fn read_test_files(
    dir: impl AsRef<Path>,
    tx: mpsc::Sender<(i64, String)>,
    shutdown_rx: watch::Receiver<bool>,
) -> Result<()> {
    let mut files: Vec<PathBuf> = fs::read_dir(dir).context("Reading directory failed.")?
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

pub async fn read_test_files_from_7z(
    path: impl AsRef<OsStr>,
    tx: mpsc::Sender<(i64, String)>,
    shutdown_rx: watch::Receiver<bool>,
) -> Result<()> {
    let archive_path = Path::new(&path);

    let file = File::open(archive_path)?;
    let len = file.metadata()?.len();

    let mut reader = SevenZReader::new(file, len, Password::empty())?;

    let mut files: Vec<(String, Vec<u8>)> = Vec::new();

    reader.for_each_entries(|entry, reader| {
        if entry.is_directory() {
            return Ok(true);
        }

        let mut content = Vec::new();
        reader.read_to_end(&mut content)?;

        files.push((entry.name().to_string(), content));

        Ok(true)
    })?;

    files.sort_by(|a, b| a.0.cmp(&b.0));

    println!("Sending {} packets...", files.len());
    for (filename, content) in files {
        if *shutdown_rx.borrow() {
            break;
        }

        let raw = String::from_utf8_lossy(&content).into_owned();

        let file_name_timestamp = Path::new(&filename)
            .file_stem()
            .expect("No file name")
            .to_str()
            .expect("This is not a string?")
            .parse()
            .expect("Not a valid i64");

        if let Err(err) = tx.send((file_name_timestamp, raw)).await {
            println!("Failed to send {}", err);
            break;
        }
    }
    Ok(())
}
