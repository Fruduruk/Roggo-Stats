use crate::core::rl_api::models::{Event, RawPacket};


pub fn deserialize_single_event(packet: &RawPacket) -> Option<Event> {
    match Event::new(packet) {
        Ok(event) => {
            // println!("Parsed Event: {event:#?}");
            Some(event)
        }
        Err(err) => {
            tracing::warn!("Error while parsing raw event, returning unknown: {}", err);
            tracing::warn!("Could not parse this packet: {packet:#?}");
            None
        }
    }
}
