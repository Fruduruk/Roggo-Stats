use crate::core::rl_api::models::RawPacket;
use crate::core::rl_api::{Error, Result};

const VECTOR_COMPACTION_THRESHOLD: usize = 1_048_576;

#[derive(Default)]
pub struct ByteBuffer {
    bytes: Vec<u8>,
    read_position: usize,
}

impl ByteBuffer {
    pub fn push(&mut self, mut bytes: Vec<u8>) {
        self.bytes.append(&mut bytes);
    }

    pub fn get(&mut self) -> Result<Option<RawPacket>> {
        let (packet, consumed) = {
            let deserializer =
                serde_json::Deserializer::from_slice(&self.bytes[self.read_position..]);
            let mut stream = deserializer.into_iter::<RawPacket>();

            if let Some(json_result) = stream.next() {
                match json_result {
                    Ok(value) => (value, stream.byte_offset()),
                    Err(error) => {
                        if error.is_eof() {
                            return Ok(None);
                        }

                        self.reset();
                        return Err(Error::SerdeError(error));
                    }
                }
            } else {
                return Ok(None);
            }
        };

        self.read_position += consumed;

        if self.read_position > VECTOR_COMPACTION_THRESHOLD {
            self.bytes.drain(..self.read_position);
            self.read_position = 0;
        }

        Ok(Some(packet))
    }

    pub fn reset(&mut self) {
        self.bytes = vec![];
        self.read_position = 0;
    }
}

#[cfg(test)]
mod tests {
    use crate::core::rl_api::{byte_buffer::{ByteBuffer, VECTOR_COMPACTION_THRESHOLD}, models::RawPacket};

    const FULL_PACKET: &[u8; 88] = include_bytes!("../../../testfiles/full_packet.json");
    const FIRST_HALF: &[u8; 29] = include_bytes!("../../../testfiles/first_half.json");
    const SECOND_HALF: &[u8; 59] = include_bytes!("../../../testfiles/second_half.json");
    const FULL_PACKET_STRING: &str = include_str!("../../../testfiles/full_packet.json");

    #[test]
    fn get_returns_none_for_incomplete_packet() {
        let mut buffer = ByteBuffer::default();
        buffer.push(FIRST_HALF.to_vec());

        assert_eq!(
            buffer
                .get()
                .expect("incomplete JSON should not cause a parsing error"),
            None
        );
    }

    #[test]
    fn get_returns_packet_for_complete_packet() {
        let mut buffer = ByteBuffer::default();
        buffer.push(FULL_PACKET.to_vec());

        let mut stream =
            serde_json::Deserializer::from_str(FULL_PACKET_STRING).into_iter::<RawPacket>();

        let control_packet = stream
            .next()
            .expect("control packet should exist")
            .expect("control packet should be valid JSON");

        assert_eq!(
            buffer
                .get()
                .expect("complete JSON should be parsed successfully"),
            Some(control_packet)
        );
    }

    #[test]
    fn get_returns_packet_and_preserves_incomplete_remainder() {
        let mut buffer = ByteBuffer::default();
        buffer.push(FULL_PACKET.to_vec());
        buffer.push(FIRST_HALF.to_vec());

        let mut stream =
            serde_json::Deserializer::from_str(FULL_PACKET_STRING).into_iter::<RawPacket>();

        let control_packet = stream
            .next()
            .expect("control packet should exist")
            .expect("control packet should be valid JSON");

        assert_eq!(
            buffer
                .get()
                .expect("complete packet should be parsed successfully"),
            Some(control_packet)
        );

        assert_eq!(
            buffer
                .get()
                .expect("incomplete remainder should not cause a parsing error"),
            None
        );
    }

    #[test]
    fn get_returns_packet_after_remainder_is_completed() {
        let mut buffer = ByteBuffer::default();

        buffer.push(FULL_PACKET.to_vec());
        buffer.push(FIRST_HALF.to_vec());

        let mut stream =
            serde_json::Deserializer::from_str(FULL_PACKET_STRING).into_iter::<RawPacket>();

        let control_packet = stream
            .next()
            .expect("control packet should exist")
            .expect("control packet should be valid JSON");

        assert_eq!(
            buffer
                .get()
                .expect("complete packet should be parsed successfully"),
            Some(control_packet.clone())
        );

        assert_eq!(
            buffer
                .get()
                .expect("incomplete remainder should not cause a parsing error"),
            None
        );

        buffer.push(SECOND_HALF.to_vec());

        assert_eq!(
            buffer
                .get()
                .expect("completed remainder should be parsed successfully"),
            Some(control_packet)
        );
    }

    #[test]
    fn get_returns_multiple_consecutive_packets() {
        let mut buffer = ByteBuffer::default();
        buffer.push(FULL_PACKET.to_vec());
        buffer.push(FULL_PACKET.to_vec());

        let mut stream =
            serde_json::Deserializer::from_str(FULL_PACKET_STRING).into_iter::<RawPacket>();

        let control_packet = stream
            .next()
            .expect("control packet should exist")
            .expect("control packet should be valid JSON");

        assert_eq!(
            buffer
                .get()
                .expect("first packet should be parsed successfully"),
            Some(control_packet.clone())
        );

        assert_eq!(
            buffer
                .get()
                .expect("second packet should be parsed successfully"),
            Some(control_packet)
        );

        assert_eq!(
            buffer
                .get()
                .expect("empty buffer should not cause a parsing error"),
            None
        );
    }

    #[test]
    fn get_returns_error_for_invalid_json() {
        let mut buffer = ByteBuffer::default();
        buffer.push(br#"{"name": invalid}"#.to_vec());

        assert!(
            buffer.get().is_err(),
            "invalid JSON should cause a parsing error"
        );
    }

    #[test]
    fn get_returns_no_error_after_invalid_json_error() {
        let mut buffer = ByteBuffer::default();
        buffer.push(br#"{"name": invalid}"#.to_vec());

        assert!(
            buffer.get().is_err(),
            "invalid JSON should cause a parsing error"
        );

        assert!(
            buffer.get().is_ok(),
            "after an error buffer should be flushed"
        );

        assert_eq!(buffer.bytes.len(), 0);
    }

    #[test]
    fn get_compacts_buffer_after_read_position_exceeds_threshold() {
        let mut buffer = ByteBuffer::default();

        let packet_count = VECTOR_COMPACTION_THRESHOLD / FULL_PACKET.len() + 1;

        for _ in 0..packet_count {
            buffer.push(FULL_PACKET.to_vec());
        }

        buffer.push(FIRST_HALF.to_vec());

        for _ in 0..packet_count {
            assert!(
                buffer
                    .get()
                    .expect("complete packet should be parsed successfully")
                    .is_some()
            );
        }

        assert_eq!(buffer.read_position, 0);
        assert_eq!(buffer.bytes, FIRST_HALF);

        assert_eq!(
            buffer
                .get()
                .expect("incomplete packet should not cause a parsing error"),
            None
        );
    }
}
