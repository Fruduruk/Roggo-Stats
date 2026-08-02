use std::{collections::HashSet, path::PathBuf};

use uuid::Uuid;

use crate::core::{
    bl::{game_stat_collector::GameStatCollector, intermediate_models},
    db::Repository,
    rl_api::{
        Result, byte_buffer::ByteBuffer, deserializer::deserialize_single_event, models::Event,
    },
};

#[derive(Debug, Default)]
pub struct Aggregator {
    byte_buffer: ByteBuffer,
    collector: Option<GameStatCollector>,
    collected_matches: HashSet<Uuid>,
    db_file_path: PathBuf,
}

impl Aggregator {
    pub fn new(db_file_path: PathBuf) -> Self {
        Self {
            db_file_path,
            ..Default::default()
        }
    }

    pub fn insert(&mut self, timestamp: i64, bytes: Vec<u8>) -> Result<()> {
        self.byte_buffer.push(bytes);
        while let Some(raw_packet) = self.byte_buffer.get()? {
            let Some(event) = deserialize_single_event(&raw_packet) else {
                continue;
            };
            self.cancel_if_outdated(event.get_match_guid());
            self.handle_event(timestamp, event)?;
            self.export_collector_if_finished()?;
        }

        Ok(())
    }

    fn export_collector_if_finished(&mut self) -> Result<()> {
        let finished = self
            .collector
            .as_ref()
            .is_some_and(|collector| collector.is_finished());

        if finished && let Some(collector) = self.collector.take() {
            self.collected_matches.insert(collector.get_match_guid());
            tracing::info!("Game {} finished.", collector.get_match_guid());
            let (stats, errors) = collector.export();

            log_stats(&stats);
            log_errors(&errors);
            let mut repository = Repository::connect(&self.db_file_path)?;

            if let Err(err) = repository.insert_game_stats(stats, errors) {
                tracing::error!(error= %err, "Failed to save match stats");
            }
        }
        Ok(())
    }

    fn cancel_if_outdated(&mut self, match_guid: Option<Uuid>) {
        if let Some(match_guid) = match_guid
            && let Some(collector) = &self.collector
            && match_guid != collector.get_match_guid()
        {
            self.collector = None;
        }
    }

    fn handle_event(&mut self, timestamp: i64, event: Event) -> Result<()> {
        let Some(match_guid) = event.get_match_guid() else {
            // println!("Discarding event, because it has no match_guid");
            return Ok(());
        };

        if self.collected_matches.contains(&match_guid) {
            // println!("Discarding event, because collection finished for match {match_guid}");
            return Ok(());
        }

        let collector = self
            .collector
            .get_or_insert_with(|| GameStatCollector::new(match_guid, timestamp));

        collector.insert(timestamp, event);
        Ok(())
    }
}

fn log_errors(errors: &[crate::core::bl::Error]) {
    for error in errors {
        tracing::warn!(err= %error, "Export partially failed.");
    }
}

fn log_stats(stats: &intermediate_models::GameStats) {
    tracing::info!(
        "Start: {} End: {} Duration: {} Had Overtime: {}",
        chrono::DateTime::from_timestamp_millis(stats.created_at_timestamp)
            .unwrap_or_default()
            .format("%d.%m.%Y %H:%M:%S"),
        chrono::DateTime::from_timestamp_millis(stats.ended_at_timestamp)
            .unwrap_or_default()
            .format("%d.%m.%Y %H:%M:%S"),
        chrono::DateTime::from_timestamp_millis(stats.duration)
            .unwrap_or_default()
            .format("%M:%S"),
        stats.had_overtime
    );
}
