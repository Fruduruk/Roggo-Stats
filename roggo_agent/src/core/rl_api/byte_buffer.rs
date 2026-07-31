use std::time::{Duration, SystemTime, UNIX_EPOCH};

use crate::WEB_UI_URL;
use crate::core::rl_api::models::RawPacket;
use crate::core::rl_api::{Error, Result};
use crate::settings::models::AgentConfig;
use tokio::io::AsyncReadExt;
use tokio::net::TcpStream;
use tokio::sync::{mpsc, watch};

#[derive(Default)]
pub struct ByteBuffer {
    bytes: Vec<u8>,
}

impl ByteBuffer {
    pub fn push(&mut self, mut bytes: Vec<u8>) {
        self.bytes.append(&mut bytes);
    }

    pub fn get(&mut self) -> Option<RawPacket> {
        let mut packet = None;
        let consumed = {
            let deserializer = serde_json::Deserializer::from_slice(&self.bytes);
            let mut stream = deserializer.into_iter::<RawPacket>();

            match stream.next() {
                Some(Ok(value)) => {
                    packet = Some(value);
                    Some(stream.byte_offset())
                }
                Some(Err(error)) if error.is_eof() => None,
                Some(Err(error)) => return None,
                None => None,
            }
        };

        if let Some(consumed) = consumed {
            self.bytes.drain(..consumed);
        }

        packet
    }
}

#[cfg(test)]
pub mod tests {
    use crate::core::rl_api::{byte_buffer::ByteBuffer, models::RawPacket};

    #[test]
    fn buffer_should_start_empty() {
        let buffer = ByteBuffer::default();
        assert_eq!(buffer.bytes, Vec::<u8>::default());
    }

    #[test]
    fn buffer_should_return_none_if_half_filled() {
        let mut buffer = ByteBuffer::default();
        let first_half = include_bytes!("../../../testfiles/first_half.json");
        buffer.push(first_half.to_vec());
        assert_eq!(buffer.get(), None);
    }

    #[test]
    fn buffer_should_return_event_if_filled() {
        let mut buffer = ByteBuffer::default();
        let full_packet = include_bytes!("../../../testfiles/full_packet.json");
        buffer.push(full_packet.to_vec());
        let full_packet_string = include_str!("../../../testfiles/full_packet.json");
        let mut stream =
            serde_json::Deserializer::from_str(&full_packet_string).into_iter::<RawPacket>();
        let control_packet = stream.next().expect("should work").expect("should work");

        assert_eq!(buffer.get(), Some(control_packet));
    }

    
    #[test]
    fn buffer_should_return_event_if_one_and_half_filled() {
        let mut buffer = ByteBuffer::default();
        let full_packet = include_bytes!("../../../testfiles/full_packet.json");
        let first_half = include_bytes!("../../../testfiles/first_half.json");
        buffer.push(full_packet.to_vec());
        buffer.push(first_half.to_vec());
        let full_packet_string = include_str!("../../../testfiles/full_packet.json");
        let mut stream =
            serde_json::Deserializer::from_str(&full_packet_string).into_iter::<RawPacket>();
        let control_packet = stream.next().expect("should work").expect("should work");

        assert_eq!(buffer.get(), Some(control_packet));
        assert_eq!(buffer.get(), None);
    }

    #[test]
    fn buffer_should_return_event_if_filled_afterwards() {
        let mut buffer = ByteBuffer::default();
        let full_packet = include_bytes!("../../../testfiles/full_packet.json");
        let first_half = include_bytes!("../../../testfiles/first_half.json");
        let second_half = include_bytes!("../../../testfiles/second_half.json");
        buffer.push(full_packet.to_vec());
        buffer.push(first_half.to_vec());
        let full_packet_string = include_str!("../../../testfiles/full_packet.json");
        let mut stream =
            serde_json::Deserializer::from_str(&full_packet_string).into_iter::<RawPacket>();
        let control_packet = stream.next().expect("should work").expect("should work");

        assert_eq!(buffer.get(), Some(control_packet.clone()));
        assert_eq!(buffer.get(), None);
        buffer.push(second_half.to_vec());
        assert_eq!(buffer.get(), Some(control_packet));
    }
}
