use crate::core::{deserializer::deserialize, game_stat_collector::GameStatCollector};
pub struct Aggregator {
    collector: GameStatCollector,
}

impl Aggregator {
    pub fn new() -> Self {
        Self {
            collector: GameStatCollector::default(),
        }
    }

    pub fn insert(&mut self, raw: String) {
        for event in deserialize(&raw) {
            self.collector.insert(event);
        }



        if self.collector.finished {
            let collector = std::mem::take(&mut self.collector);
            let stats = collector.export();
            println!("Stats for this game: {stats:#?}");
        }
    }
}
