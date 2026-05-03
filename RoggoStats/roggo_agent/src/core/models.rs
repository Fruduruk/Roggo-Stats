use uuid::Uuid;

#[derive(Debug, serde::Deserialize)]
pub struct Packet {
    #[serde(rename = "Event")]
    pub event: Event,

    #[serde(rename = "Data")]
    pub data: String,
}

#[derive(PartialEq, Eq, Hash, Debug, Clone, Copy, serde::Deserialize)]
pub enum Event {
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
