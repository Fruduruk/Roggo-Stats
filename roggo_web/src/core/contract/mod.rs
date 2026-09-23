pub mod day;
pub mod session;
pub mod single_match;

use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize)]
pub struct VersionDto {
    pub version: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct MainCharacterDto {
    pub username: String,
    pub primary_id: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub enum AgentErrorCode {
    NoEntries,
    InternalError,
    UserError,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct AgentErrorDto {
    pub error: AgentErrorCode,
    pub message: String,
    pub details: Vec<String>,
}

impl std::fmt::Display for Playlist {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_str(match self {
            Playlist::Unknown => "Unknown",
            Playlist::Duel => "Duel",
            Playlist::Doubles => "Doubles",
            Playlist::Standard => "Standard",
            Playlist::Chaos => "Chaos",
            Playlist::RankedDuel => "Ranked Duel",
            Playlist::RankedDoubles => "Ranked Doubles",
            Playlist::RankedStandard => "Ranked Standard",
            Playlist::SnowDay => "Snow Day",
            Playlist::RocketLabs => "Rocket Labs",
            Playlist::Hoops => "Hoops",
            Playlist::Rumble => "Rumble",
            Playlist::TournamentMatch => "Tournament Match",
            Playlist::Dropshot => "Dropshot",
            Playlist::ExternalMatch => "External Match",
            Playlist::RankedHoops => "Ranked Hoops",
            Playlist::RankedRumble => "Ranked Rumble",
            Playlist::RankedDropshot => "Ranked Dropshot",
            Playlist::RankedSnowDay => "Ranked Snow Day",
            Playlist::GhostHunt => "Ghost Hunt",
            Playlist::BeachBall => "Beach Ball",
            Playlist::SpikeRush => "Spike Rush",
            Playlist::TournamentMatch34 => "Tournament Match 34",
            Playlist::RocketLabs35 => "Rocket Labs 35",
            Playlist::DropshotRumble => "Dropshot Rumble",
            Playlist::Heatseeker => "Heatseeker",
            Playlist::BoomerBall => "Boomer Ball",
            Playlist::HeatseekerDoubles => "Heatseeker Doubles",
            Playlist::WinterBreakaway => "Winter Breakaway",
            Playlist::Gridiron => "Gridiron",
            Playlist::SuperCube => "Super Cube",
            Playlist::TacticalRumble => "Tactical Rumble",
            Playlist::SpringLoaded => "Spring Loaded",
            Playlist::SpeedDemon => "Speed Demon",
            Playlist::GothamCityRumble => "Gotham City Rumble",
            Playlist::Knockout => "Knockout",
            Playlist::ConfidentialThirdWheelTest => "Confidential Third Wheel Test",
            Playlist::NikeFcShowdown => "Nike FC Showdown",
            Playlist::HauntedHeatseekerDoubles => "Haunted Heatseeker Doubles",
            Playlist::HauntedHeatseeker => "Haunted Heatseeker",
            Playlist::HeatseekerRicochet => "Heatseeker Ricochet",
            Playlist::SpookyCube => "Spooky Cube",
            Playlist::GForceFrenzy => "G-Force Frenzy",
            Playlist::DropshotRumbleDoubles => "Dropshot Rumble Doubles",
        })
    }
}

#[derive(Default, Debug, Clone, Copy, PartialEq, Eq, Hash, Serialize, Deserialize)]
#[repr(u32)]
pub enum Playlist {
    #[default]
    Unknown = 0,
    Duel = 1,
    Doubles = 2,
    Standard = 3,
    Chaos = 4,

    RankedDuel = 10,
    RankedDoubles = 11,
    RankedStandard = 13,

    SnowDay = 15,
    RocketLabs = 16,
    Hoops = 17,
    Rumble = 18,

    TournamentMatch = 22,
    Dropshot = 23,
    ExternalMatch = 26,

    RankedHoops = 27,
    RankedRumble = 28,
    RankedDropshot = 29,
    RankedSnowDay = 30,

    GhostHunt = 31,
    BeachBall = 32,
    SpikeRush = 33,

    TournamentMatch34 = 34,
    RocketLabs35 = 35,

    DropshotRumble = 37,
    Heatseeker = 38,
    BoomerBall = 41,
    HeatseekerDoubles = 43,
    WinterBreakaway = 44,
    Gridiron = 46,
    SuperCube = 47,
    TacticalRumble = 48,
    SpringLoaded = 49,
    SpeedDemon = 50,
    GothamCityRumble = 52,
    Knockout = 54,

    ConfidentialThirdWheelTest = 55,

    NikeFcShowdown = 62,
    HauntedHeatseekerDoubles = 64,
    HauntedHeatseeker = 65,
    HeatseekerRicochet = 66,
    SpookyCube = 67,
    GForceFrenzy = 68,

    DropshotRumbleDoubles = 70,
}

impl From<u32> for Playlist {
    fn from(id: u32) -> Self {
        match id {
            1 => Self::Duel,
            2 => Self::Doubles,
            3 => Self::Standard,
            4 => Self::Chaos,
            10 => Self::RankedDuel,
            11 => Self::RankedDoubles,
            13 => Self::RankedStandard,
            15 => Self::SnowDay,
            16 => Self::RocketLabs,
            17 => Self::Hoops,
            18 => Self::Rumble,
            22 => Self::TournamentMatch,
            23 => Self::Dropshot,
            26 => Self::ExternalMatch,
            27 => Self::RankedHoops,
            28 => Self::RankedRumble,
            29 => Self::RankedDropshot,
            30 => Self::RankedSnowDay,
            31 => Self::GhostHunt,
            32 => Self::BeachBall,
            33 => Self::SpikeRush,
            34 => Self::TournamentMatch34,
            35 => Self::RocketLabs35,
            37 => Self::DropshotRumble,
            38 => Self::Heatseeker,
            41 => Self::BoomerBall,
            43 => Self::HeatseekerDoubles,
            44 => Self::WinterBreakaway,
            46 => Self::Gridiron,
            47 => Self::SuperCube,
            48 => Self::TacticalRumble,
            49 => Self::SpringLoaded,
            50 => Self::SpeedDemon,
            52 => Self::GothamCityRumble,
            54 => Self::Knockout,
            55 => Self::ConfidentialThirdWheelTest,
            62 => Self::NikeFcShowdown,
            64 => Self::HauntedHeatseekerDoubles,
            65 => Self::HauntedHeatseeker,
            66 => Self::HeatseekerRicochet,
            67 => Self::SpookyCube,
            68 => Self::GForceFrenzy,
            70 => Self::DropshotRumbleDoubles,

            _ => Self::Unknown,
        }
    }
}

impl Playlist {
    pub const fn id(self) -> u32 {
        self as u32
    }

    pub const fn is_ranked(self) -> bool {
        matches!(
            self,
            Self::RankedDuel
                | Self::RankedDoubles
                | Self::RankedStandard
                | Self::TournamentMatch
                | Self::ExternalMatch
                | Self::RankedHoops
                | Self::RankedRumble
                | Self::RankedDropshot
                | Self::RankedSnowDay
                | Self::TournamentMatch34
        )
    }
}

// #[derive(Debug, Serialize, Deserialize)]
// pub struct HideRequest {
//     pub match_guid: Uuid,
//     pub hide: bool,
// }