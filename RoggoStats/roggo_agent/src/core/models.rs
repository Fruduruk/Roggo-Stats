use std::error::Error;

use uuid::Uuid;

#[derive(Debug, serde::Deserialize)]
pub struct RawPacket {
    #[serde(rename = "Event")]
    pub event: RawEvent,

    #[serde(rename = "Data")]
    pub data: String,
}

#[derive(PartialEq, Eq, Hash, Debug, Clone, Copy, serde::Deserialize)]
pub enum RawEvent {
    UpdateState,         // UpdateState:441
    BallHit,             // BallHit:217
    ClockUpdatedSeconds, // ClockUpdatedSeconds:301
    CountdownBegin,      // CountdownBegin:9
    CrossbarHit,         // CrossbarHit:2
    GoalReplayEnd,
    GoalReplayStart,
    GoalReplayWillEnd,
    GoalScored,       // GoalScored:16
    MatchCreated,     // MatchCreated:1
    MatchInitialized, // MatchInitialized:1
    MatchDestroyed,   // MatchDestroyed:1
    MatchEnded,       // MatchEnded:1
    MatchPaused,
    MatchUnpaused,
    PodiumStart, // PodiumStart:1
    ReplayCreated,
    RoundStarted,  // RoundStarted:9
    StatfeedEvent, // StatfeedEvent:38
}

#[derive(PartialEq, Debug, Clone)]
pub enum Event {
    UpdateState,
    BallHit(BallHit),
    ClockUpdatedSeconds,
    CountdownBegin,
    CrossbarHit,
    GoalReplayEnd,
    GoalReplayStart,
    GoalReplayWillEnd,
    GoalScored,
    MatchCreated,
    MatchInitialized,
    MatchDestroyed,
    MatchEnded,
    MatchPaused,
    MatchUnpaused,
    PodiumStart,
    ReplayCreated,
    RoundStarted,
    StatfeedEvent,
}

impl Event {
    pub fn new(raw_packet: RawPacket) -> Result<Event, Box<dyn Error>> {
        Ok(match raw_packet.event {
            RawEvent::UpdateState => Event::UpdateState,
            RawEvent::BallHit => Event::BallHit(serde_json::from_str(&raw_packet.data)?),
            RawEvent::ClockUpdatedSeconds => Event::ClockUpdatedSeconds,
            RawEvent::CountdownBegin => Event::CountdownBegin,
            RawEvent::CrossbarHit => Event::CrossbarHit,
            RawEvent::GoalReplayEnd => Event::GoalReplayEnd,
            RawEvent::GoalReplayStart => Event::GoalReplayStart,
            RawEvent::GoalReplayWillEnd => Event::GoalReplayWillEnd,
            RawEvent::GoalScored => Event::GoalScored,
            RawEvent::MatchCreated => Event::MatchCreated,
            RawEvent::MatchInitialized => Event::MatchInitialized,
            RawEvent::MatchDestroyed => Event::MatchDestroyed,
            RawEvent::MatchEnded => Event::MatchEnded,
            RawEvent::MatchPaused => Event::MatchPaused,
            RawEvent::MatchUnpaused => Event::MatchUnpaused,
            RawEvent::PodiumStart => Event::PodiumStart,
            RawEvent::ReplayCreated => Event::ReplayCreated,
            RawEvent::RoundStarted => Event::RoundStarted,
            RawEvent::StatfeedEvent => Event::StatfeedEvent,
        })
    }
}

#[derive(PartialEq, Debug, Clone, Copy, serde::Deserialize)]
pub struct Location {
    #[serde(rename = "X")]
    pub x: f64,
    #[serde(rename = "Y")]
    pub y: f64,
    #[serde(rename = "Z")]
    pub z: f64,
}

#[derive(PartialEq, Debug, Clone, Copy, serde::Deserialize)]
pub struct Ball {
    #[serde(rename = "PreHitSpeed")]
    pub pre_hit_speed: f64,
    #[serde(rename = "PostHitSpeed")]
    pub post_hit_speed: f64,
    #[serde(rename = "Location")]
    pub location: Location,
}

#[derive(PartialEq, Eq, Hash, Debug, Clone, serde::Deserialize)]
pub struct Player {
    #[serde(rename = "Name")]
    pub name: String,
    #[serde(rename = "Shortcut")]
    pub shortcut: i16,
    #[serde(rename = "TeamNum")]
    pub team_num: u8,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct BallHit {
    #[serde(rename = "Players")]
    pub players: Vec<Player>,
    #[serde(rename = "Ball")]
    pub ball: Ball,
    #[serde(rename = "MatchGuid")]
    pub match_guid: Uuid,
}
