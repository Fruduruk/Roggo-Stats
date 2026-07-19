use serde::{Deserialize, Serialize};


#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub struct AgentConfig {
    pub rl_api_port: u16,
    pub start_ui_when_rl_closes: bool,
}

impl Default for AgentConfig {
    fn default() -> Self {
        Self {
            rl_api_port: 49123,
            start_ui_when_rl_closes: false,
        }
    }
}

#[derive(Debug, Deserialize)]
pub struct TAStatsAPI {
    #[serde(rename = "TAGame")]
    pub ta_game: TaGame,
}

impl TAStatsAPI {
    #[inline]
    pub fn get_port(&self) -> u16 {
        self.ta_game.match_stats_exporter.port
    }
    #[inline]
    pub fn get_packet_send_rate(&self) -> u32 {
        self.ta_game.match_stats_exporter.packet_send_rate
    }
}

#[derive(Debug, Deserialize)]
pub struct TaGame {
    #[serde(rename = "MatchStatsExporter_TA")]
    pub match_stats_exporter: MatchStatsExporter,
}

#[derive(Debug, Deserialize)]
pub struct MatchStatsExporter {
    #[serde(rename = "Port")]
    pub port: u16,

    #[serde(rename = "PacketSendRate")]
    pub packet_send_rate: u32,
}