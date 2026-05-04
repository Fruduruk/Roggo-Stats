use serde::de::Error as SerdeError;
use serde::{Deserialize, Deserializer};
use std::error::Error;
use uuid::Uuid;

fn empty_string_as_none_uuid<'de, D>(deserializer: D) -> Result<Option<Uuid>, D::Error>
where
    D: Deserializer<'de>,
{
    let value = Option::<String>::deserialize(deserializer)?;

    match value.as_deref() {
        None | Some("") => Ok(None),
        Some(s) => Uuid::parse_str(s).map(Some).map_err(D::Error::custom),
    }
}

#[derive(PartialEq, Eq, Hash, Debug, Clone, serde::Deserialize)]
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
    ReplayWillEnd,
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
    UpdateState(UpdateState),
    BallHit(BallHit),
    ClockUpdatedSeconds(ClockUpdatedSeconds),
    CountdownBegin(MatchIdentfier),
    CrossbarHit(CrossbarHit),
    GoalReplayEnd(MatchIdentfier),
    GoalReplayStart(MatchIdentfier),
    GoalReplayWillEnd(MatchIdentfier),
    ReplayWillEnd(MatchIdentfier),
    GoalScored(GoalScored),
    MatchCreated(MatchIdentfier),
    MatchInitialized(MatchIdentfier),
    MatchDestroyed(MatchIdentfier),
    MatchEnded(MatchEnded),
    MatchPaused(MatchIdentfier),
    MatchUnpaused(MatchIdentfier),
    PodiumStart(MatchIdentfier),
    ReplayCreated(MatchIdentfier),
    RoundStarted(MatchIdentfier),
    StatfeedEvent(StatfeedEvent),
}

impl Event {
    pub fn new(raw_packet: &RawPacket) -> Result<Event, Box<dyn Error>> {
        Ok(match raw_packet.event {
            RawEvent::UpdateState => Event::UpdateState(serde_json::from_str(&raw_packet.data)?),
            RawEvent::BallHit => Event::BallHit(serde_json::from_str(&raw_packet.data)?),
            RawEvent::ClockUpdatedSeconds => {
                Event::ClockUpdatedSeconds(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::CountdownBegin => {
                Event::CountdownBegin(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::CrossbarHit => Event::CrossbarHit(serde_json::from_str(&raw_packet.data)?),
            RawEvent::GoalReplayEnd => {
                Event::GoalReplayEnd(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::GoalReplayStart => {
                Event::GoalReplayStart(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::GoalReplayWillEnd => {
                Event::GoalReplayWillEnd(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::GoalScored => Event::GoalScored(serde_json::from_str(&raw_packet.data)?),
            RawEvent::MatchCreated => Event::MatchCreated(serde_json::from_str(&raw_packet.data)?),
            RawEvent::MatchInitialized => {
                Event::MatchInitialized(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::MatchDestroyed => {
                Event::MatchDestroyed(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::MatchEnded => Event::MatchEnded(serde_json::from_str(&raw_packet.data)?),
            RawEvent::MatchPaused => Event::MatchPaused(serde_json::from_str(&raw_packet.data)?),
            RawEvent::MatchUnpaused => {
                Event::MatchUnpaused(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::PodiumStart => Event::PodiumStart(serde_json::from_str(&raw_packet.data)?),
            RawEvent::ReplayCreated => {
                Event::ReplayCreated(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::RoundStarted => Event::RoundStarted(serde_json::from_str(&raw_packet.data)?),
            RawEvent::StatfeedEvent => {
                Event::StatfeedEvent(serde_json::from_str(&raw_packet.data)?)
            }
            RawEvent::ReplayWillEnd => {
                Event::ReplayWillEnd(serde_json::from_str(&raw_packet.data)?)
            }
        })
    }
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct UpdateState {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
    #[serde(rename = "Players")]
    pub players: Vec<Player>,
    #[serde(rename = "Game")]
    pub game: Game,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct Player {
    #[serde(rename = "Name")]
    pub name: String,
    #[serde(rename = "PrimaryId")]
    pub primary_id: String,
    #[serde(rename = "Shortcut")]
    pub shortcut: u8,
    #[serde(rename = "TeamNum")]
    pub team_num: u8,
    #[serde(rename = "Score")]
    pub score: u16,
    #[serde(rename = "Goals")]
    pub goals: u16,
    #[serde(rename = "Shots")]
    pub shots: u16,
    #[serde(rename = "Assists")]
    pub assists: u16,
    #[serde(rename = "Saves")]
    pub saves: u16,
    #[serde(rename = "Touches")]
    pub touches: u32,
    #[serde(rename = "CarTouches")]
    pub car_touches: u32,
    #[serde(rename = "Demos")]
    pub demos: u16,
    #[serde(rename = "bHasCar")]
    pub b_has_car: Option<bool>,
    #[serde(rename = "Speed")]
    pub speed: Option<f64>,
    #[serde(rename = "Boost")]
    pub boost: Option<f64>,
    #[serde(rename = "bBoosting")]
    pub b_boosting: Option<bool>,
    #[serde(rename = "bOnGround")]
    pub b_on_ground: Option<bool>,
    #[serde(rename = "bOnWall")]
    pub b_on_wall: Option<bool>,
    #[serde(rename = "bPowersliding")]
    pub b_powersliding: Option<bool>,
    #[serde(rename = "bDemolished")]
    pub b_demolished: Option<bool>,
    #[serde(rename = "bSupersonic")]
    pub b_supersonic: Option<bool>,
    #[serde(rename = "Attacker")]
    pub attacker: Option<GamePlayer>,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct Game {
    #[serde(rename = "Teams")]
    pub teams: Vec<Team>,
    #[serde(rename = "TimeSeconds")]
    pub time_seconds: i32,
    #[serde(rename = "bOvertime")]
    pub b_overtime: bool,
    #[serde(rename = "Ball")]
    pub ball_state: BallState,
    #[serde(rename = "bReplay")]
    pub b_replay: bool,
    #[serde(rename = "bHasWinner")]
    pub b_has_winner: bool,
    #[serde(rename = "Winner")]
    pub winner: String,
    #[serde(rename = "Arena")]
    pub arena: String,
    #[serde(rename = "bHasTarget")]
    pub b_has_target: bool,
    #[serde(rename = "Target")]
    pub target: Option<GamePlayer>,
    #[serde(rename = "Frame")]
    pub frame: Option<u64>,
    #[serde(rename = "Elapsed")]
    pub elapsed: Option<f64>,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct Team {
    #[serde(rename = "Name")]
    pub name: String,
    #[serde(rename = "TeamNum")]
    pub team_num: u8,
    #[serde(rename = "Score")]
    pub score: u16,
    #[serde(rename = "ColorPrimary")]
    pub color_primary: String,
    #[serde(rename = "ColorSecondary")]
    pub color_secondary: String,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct BallState {
    #[serde(rename = "Speed")]
    pub speed: f64,
    #[serde(rename = "TeamNum")]
    pub team_num: u8,
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
pub struct GamePlayer {
    #[serde(rename = "Name")]
    pub name: String,
    #[serde(rename = "Shortcut")]
    pub shortcut: u8,
    #[serde(rename = "TeamNum")]
    pub team_num: u8,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct BallHit {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
    #[serde(rename = "Players")]
    pub players: Vec<GamePlayer>,
    #[serde(rename = "Ball")]
    pub ball: Ball,
}

#[derive(PartialEq, Eq, Hash, Debug, Clone, serde::Deserialize)]
pub struct ClockUpdatedSeconds {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
    #[serde(rename = "TimeSeconds")]
    pub time_seconds: u16,
    #[serde(rename = "bOvertime")]
    pub b_overtime: bool,
}

#[derive(PartialEq, Eq, Hash, Debug, Clone, Copy, serde::Deserialize)]
pub struct MatchIdentfier {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct BallLastTouch {
    #[serde(rename = "Player")]
    player: GamePlayer,
    #[serde(rename = "Speed")]
    speed: f64,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct CrossbarHit {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
    #[serde(rename = "BallLocation")]
    pub ball_location: Location,
    #[serde(rename = "BallSpeed")]
    pub ball_speed: f64,
    #[serde(rename = "ImpactForce")]
    pub impact_force: f64,
    #[serde(rename = "BallLastTouch")]
    pub ball_last_touch: BallLastTouch,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct GoalScored {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
    #[serde(rename = "GoalSpeed")]
    pub goal_speed: f64,
    #[serde(rename = "GoalTime")]
    pub goal_time: f64,
    #[serde(rename = "ImpactLocation")]
    pub impact_location: Location,
    #[serde(rename = "Scorer")]
    pub scorer: GamePlayer,
    #[serde(rename = "Assister")]
    pub assister: Option<GamePlayer>,
    #[serde(rename = "BallLastTouch")]
    pub ball_last_touch: BallLastTouch,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct MatchEnded {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
    #[serde(rename = "WinnerTeamNum")]
    pub winner_team_num: u8,
}

#[derive(PartialEq, Debug, Clone, serde::Deserialize)]
pub struct StatfeedEvent {
    #[serde(
        rename = "MatchGuid",
        default,
        deserialize_with = "empty_string_as_none_uuid"
    )]
    pub match_guid: Option<Uuid>,
    #[serde(rename = "EventName")]
    pub event_name: String,
    #[serde(rename = "Type")]
    pub event_type: String,
    #[serde(rename = "MainTarget")]
    pub main_target: GamePlayer,
    #[serde(rename = "SecondaryTarget")]
    pub secondary_target: Option<GamePlayer>,
}
