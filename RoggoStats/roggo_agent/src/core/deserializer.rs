use crate::core::models::api_models::{Event, RawPacket};

pub fn deserialize(raw: &String) -> Vec<Event> {
    let mut events = vec![];
    let stream = serde_json::Deserializer::from_str(&raw).into_iter::<RawPacket>();

    for json in stream {
        match json {
            Ok(raw) => {
                if let Some(event) = deserialize_single_event(&raw) {
                    events.push(event);
                }
            }
            Err(err) => {
                println!("Error while parsing next tcp packet {err}");
            }
        }
    }

    events
}

pub fn deserialize_single_event(packet: &RawPacket) -> Option<Event> {
    match Event::new(&packet) {
        Ok(event) => {
            // println!("Parsed Event: {event:#?}");
            Some(event)
        }
        Err(err) => {
            println!("Error while parsing raw event, returning unknown: {}", err);
            println!("Could not parse this packet: {packet:#?}");
            None
        }
    }
}
