use std::collections::HashMap;

use serde::Deserialize;
use serde_json::Deserializer;

use crate::core::models::{BallHit, Event, Packet};
pub struct Aggregator {
    count: u64,
    history: HashMap<Event, u32>,
}

impl Aggregator {
    pub fn new() -> Self {
        Self {
            count: 0,
            history: HashMap::new(),
        }
    }

    pub fn next(&mut self, raw: String) {
        let stream = serde_json::Deserializer::from_str(&raw).into_iter::<Packet>();

        for json in stream {
            if let Ok(raw) = json {
                self.next_packet(raw);
            } else {
                println!("Error while parsing next tcp packet");
            }
        }
    }

    pub fn next_packet(&mut self, packet: Packet) {
        let event = packet.event;
        // println!("Parsed Event {}: {event:?}", self.count);

        *self.history.entry(event).or_insert(0) += 1;

        // println!("History:");
        for (event, count) in &self.history {
            // println!("{event:?}:{count}");
        }
        if packet.event == Event::BallHit {
            let result = serde_json::from_str::<BallHit>(&packet.data);
            if let Ok(data) = result {
                println!("{data:?}");
            }
        }

        self.count += 1;
    }
}
