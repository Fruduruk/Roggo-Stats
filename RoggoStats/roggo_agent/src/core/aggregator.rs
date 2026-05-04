use crate::core::{
    deserializer::deserialize, game_stat_collector::GameStatCollector, models::api_models::Event,
};
pub struct Aggregator {
    collector: Option<GameStatCollector>,
}

impl Aggregator {
    pub fn new() -> Self {
        Self { collector: None }
    }

    pub fn insert(&mut self, raw: String) {
        for event in deserialize(&raw) {
            self.handle_event(event);
            let finished = self
                .collector
                .as_ref()
                .is_some_and(|collector| collector.is_finished());

            if finished {
                if let Some(collector) = self.collector.take() {
                    println!("Game {} finished.", collector.get_match_guid());
                    let stats = collector.export();
                    println!("Stats: {:#?}", stats);
                }
            }
        }
    }

    fn handle_event(&mut self, event: Event) {
        if let Some(match_guid) = event.get_match_guid() {
            let collector = self
                .collector
                .get_or_insert_with(|| GameStatCollector::new(match_guid));

            if collector.get_match_guid() == match_guid {
                collector.insert(event);
            } else {
                println!(
                    "Discarding event, because the agent is still collecting for a different game: {}, new event match guid: {}",
                    collector.get_match_guid(),
                    match_guid
                );
            }
        } else {
            println!("Discarding event, because it has no match_guid: {event:#?}");
        }
    }
}
