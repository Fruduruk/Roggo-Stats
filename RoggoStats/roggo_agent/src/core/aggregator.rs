use std::collections::HashMap;

use serde::Deserialize;
use serde_json::Deserializer;

use crate::core::models::{BallHit, Event, RawEvent, RawPacket};
pub struct Aggregator {
    count: u64,
    history: HashMap<RawEvent, u32>,
}

impl Aggregator {
    pub fn new() -> Self {
        Self {
            count: 0,
            history: HashMap::new(),
        }
    }

    pub fn next(&mut self, raw: String) {
        let stream = serde_json::Deserializer::from_str(&raw).into_iter::<RawPacket>();

        for json in stream {
            if let Ok(raw) = json {
                *self.history.entry(raw.event).or_insert(0) += 1;
                self.next_packet(raw);
            } else {
                println!("Error while parsing next tcp packet");
            }
        }
    }

    pub fn next_packet(&mut self, packet: RawPacket) {
        let event = match Event::new(packet) {
            Ok(event) => event,
            Err(err) => {
                println!("Error while parsing raw event: {}", err);
                return;
            }
        };
        println!("Parsed Event {}: {event:#?}", self.count);

        // if let Event::BallHit(data) = event {
        //     println!("{data:#?}");
        // }
        self.count += 1;
    }
}
