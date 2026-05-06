use std::collections::HashSet;

use uuid::Uuid;

use crate::core::{
    deserializer::deserialize, game_stat_collector::GameStatCollector, models::api_models::Event,
};
pub struct Aggregator {
    collector: Option<GameStatCollector>,
    collected_matches: HashSet<Uuid>,
}

impl Aggregator {
    pub fn new() -> Self {
        Self {
            collector: None,
            collected_matches: HashSet::new(),
        }
    }

    pub fn insert(&mut self, timestamp: u128, raw: String) {
        for event in deserialize(&raw) {
            self.cancel_if_outdated(event.get_match_guid());
            self.handle_event(timestamp, event);
            self.export_collector_if_finished();
        }
    }

    fn export_collector_if_finished(&mut self) {
        let finished = self
            .collector
            .as_ref()
            .is_some_and(|collector| collector.is_finished());

        if finished {
            if let Some(collector) = self.collector.take() {
                self.collected_matches.insert(collector.get_match_guid());
                println!("Game {} finished.", collector.get_match_guid());
                let stats = collector.export();
                println!("Stats: {:#?}", stats);
            }
        }
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

    fn handle_event(&mut self, timestamp: u128, event: Event) {
        let Some(match_guid) = event.get_match_guid() else {
            println!("Discarding event, because it has no match_guid");
            return;
        };

        if self.collected_matches.contains(&match_guid) {
            println!("Discarding event, because collection finished for match {match_guid}");
            return;
        }

        let collector = self
            .collector
            .get_or_insert_with(|| GameStatCollector::new(match_guid));

        collector.insert(timestamp, event);
    }
}
