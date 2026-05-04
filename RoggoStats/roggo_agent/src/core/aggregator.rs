use std::collections::HashMap;


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
            match json {
                Ok(raw) => {
                    *self.history.entry(raw.event).or_insert(0) += 1;
                    self.next_packet(raw);
                }
                Err(err) => {
                    println!("Error while parsing next tcp packet {err}");
                }
            }
        }
    }

    pub fn next_packet(&mut self, packet: RawPacket) {
        let save = packet.clone();
        let event = match Event::new(packet) {
            Ok(event) => event,
            Err(err) => {
                println!("Error while parsing raw event: {}", err);
                println!("Could not parse this packet: {save:#?}");
                return;
            }
        };
        println!("Parsed Event {}: {event:#?}", self.count);

        self.count += 1;
    }
}
