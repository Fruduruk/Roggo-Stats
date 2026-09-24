use crate::core::{
    Error, api_result::APIResult, app::COMPATIBLE_AGENT_VERSION, app_state::{agent_state::AgentState, parameters::Parameters}, contract::{
        AgentErrorDto,
        day::{DayDto, DaysPlayedDto},
        session::{DetailedSessionDto, SessionDto},
    },
};

pub mod agent_state;
pub mod parameters;

#[derive(Default)]
pub struct AppState {
    pub agent_state: AgentState,
    pub player_name: Option<String>,
    pub errors: Vec<AgentErrorDto>,
    pub general_errors: Vec<Error>,
    pub days_played: Option<Vec<jiff::civil::Date>>,
    pub day: Option<DayDto>,
    pub session: Option<SessionDto>,
    pub parameters: Parameters,
}

impl AppState {
    pub fn insert(&mut self, api_result: APIResult) {
        match api_result {
            APIResult::PlayerName(name) => self.player_name = Some(name),
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
            APIResult::DetailedSession(session_dto) => self.session = Some(session_dto),
            APIResult::DaysPlayed(days_played_dto) => self.days_played = Some(days_played_dto),
            APIResult::GeneralError(error) => self.general_errors.push(error),
        }
    }
}
