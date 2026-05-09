use std::collections::HashSet;

use uuid::Uuid;

use crate::core::{
    bl::game_stat_collector::GameStatCollector,
    db::repository::Repository,
    rl_api::{deserializer::deserialize, error::Result, models::Event},
};
pub struct Aggregator {
    collector: Option<GameStatCollector>,
    collected_matches: HashSet<Uuid>,
    repository: Repository,
}

impl Aggregator {
    pub fn new() -> Result<Self> {
        let repository = Repository::new("test.db")?;

        Ok(Self {
            collector: None,
            collected_matches: HashSet::new(),
            repository,
        })
    }

    pub fn insert(&mut self, timestamp: i64, raw: String) -> Result<()> {
        for event in deserialize(&raw) {
            self.cancel_if_outdated(event.get_match_guid());
            self.handle_event(timestamp, event);
            self.export_collector_if_finished()?;
        }
        Ok(())
    }

    fn export_collector_if_finished(&mut self) -> Result<()> {
        let finished = self
            .collector
            .as_ref()
            .is_some_and(|collector| collector.is_finished());

        if finished {
            if let Some(collector) = self.collector.take() {
                self.collected_matches.insert(collector.get_match_guid());
                tracing::debug!("Game {} finished.", collector.get_match_guid());
                let stats = collector.export();

                tracing::debug!(
                    "Start: {} End: {} Duration: {} Had Overtime: {}",
                    chrono::DateTime::from_timestamp_millis(stats.created_at_timestamp)
                        .unwrap()
                        .format("%d.%m.%Y %H:%M:%S"),
                    chrono::DateTime::from_timestamp_millis(stats.ended_at_timestamp)
                        .unwrap()
                        .format("%d.%m.%Y %H:%M:%S"),
                    chrono::DateTime::from_timestamp_millis(stats.duration)
                        .unwrap()
                        .format("%M:%S"),
                    stats.had_overtime
                );

                tracing::debug!(
                    "Excluded timeline instants: {}",
                    stats.excluded_timeline_instants
                );

                if let Err(err) = self.repository.insert_game_stats(stats) {
                    tracing::error!(error= %err, "failed to save match stats");
                }
            }
        }
        Ok(())
    }

    fn cancel_if_outdated(&mut self, match_guid: Option<Uuid>) {
        if let Some(match_guid) = match_guid {
            if let Some(collector) = &self.collector {
                if match_guid != collector.get_match_guid() {
                    self.collector = None;
                }
            }
        }
    }

    fn handle_event(&mut self, timestamp: i64, event: Event) {
        let Some(match_guid) = event.get_match_guid() else {
            // println!("Discarding event, because it has no match_guid");
            return;
        };

        if self.collected_matches.contains(&match_guid) {
            // println!("Discarding event, because collection finished for match {match_guid}");
            return;
        }

        let collector = self
            .collector
            .get_or_insert_with(|| GameStatCollector::new(match_guid, timestamp));

        collector.insert(timestamp, event);
    }
}
