use std::path::PathBuf;

use crate::core::{Error, Result};
use notify::{Event, EventKind, RecursiveMode, Watcher};
use tokio::sync::mpsc;

pub async fn wait_until_changed(file_path: PathBuf) -> Result<()> {
    let parent = file_path
        .parent()
        .ok_or_else(|| Error::ConfigError("File path has no parent".into()))?
        .to_path_buf();
    let file_name = file_path
        .file_name()
        .ok_or_else(|| Error::ConfigError("File path has no file name".into()))?
        .to_os_string();

    let (tx, mut rx) = mpsc::channel(16);

    let mut watcher = notify::recommended_watcher(move |event: notify::Result<Event>| {
        let _ = tx.blocking_send(event);
    })?;

    watcher.watch(&parent, RecursiveMode::NonRecursive)?;

    while let Some(event) = rx.recv().await {
        let event = event?;

        let touches_target_file = event.paths.iter().any(|path| path.file_name() == Some(&file_name));
        
        let relevant_change = matches!(
            event.kind,
            EventKind::Modify(_) | EventKind::Create(_) | EventKind::Remove(_)
        );

        if touches_target_file && relevant_change {
            return Ok(());
        }
    }

    Err(Error::ConfigError("File watcher stopped".into()))
}
