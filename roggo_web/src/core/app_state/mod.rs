use roggo_contract::*;
use crate::core::{
    Error, api_result::APIResult, app::COMPATIBLE_AGENT_VERSION, app_state::{agent_state::AgentState, parameters::Parameters},
};

pub mod agent_state;
pub mod parameters;

#[derive(Default)]
pub struct AppState {
    pub agent_state: AgentState,
    pub main_character: Option<PlayerDto>,
    pub errors: Vec<AgentErrorDto>,
    pub general_errors: Vec<Error>,
    pub days_played: Option<Vec<jiff::civil::Date>>,
    pub day: Option<DayDto>,
    pub session: Option<SessionDto>,
    pub session_details: Option<SessionDetails>,
    pub parameters: Parameters,
}

impl AppState {
    pub fn insert(&mut self, api_result: APIResult) {
        match api_result {
            APIResult::PlayerName(player_dto) => self.main_character = Some(player_dto),
            APIResult::AgentError(agent_error_dto) => self.errors.push(agent_error_dto),
            APIResult::Version(version) => match version {
                Some(version) => {
                    if version == COMPATIBLE_AGENT_VERSION {
                        self.agent_state = AgentState::Ready(version)
                    } else {
                        self.agent_state = AgentState::AgentOutdated(version)
                    }
                }
                None => self.agent_state = AgentState::AgentMissing,
            },
            APIResult::Day(day) => self.day = Some(day),
            APIResult::Session(session_dto) => self.session = Some(session_dto),
            APIResult::DaysPlayed(days_played_dto) => self.days_played = Some(days_played_dto),
            APIResult::GeneralError(error) => self.general_errors.push(error),
            APIResult::SessionDetails(session_details) => self.session_details = Some(session_details),
        }
    }
}
