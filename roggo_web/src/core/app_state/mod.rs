use crate::core::{api_result::APIResult, app::COMPATIBLE_AGENT_VERSION, app_state::agent_state::AgentState, contract::AgentErrorDto};

pub mod agent_state;

#[derive(Default)]
pub struct AppState {
    pub agent_state: AgentState,
    pub player_name: Option<String>,
    pub errors: Vec<AgentErrorDto>
}

impl AppState {
    pub fn insert(&mut self, api_result: APIResult) {
        match api_result {
            APIResult::PlayerName(name) => self.player_name = Some(name),
            APIResult::AgentError(agent_error_dto) => self.errors.push(agent_error_dto),
            APIResult::Version(version) => {
                match version {
                    Some(version) => {
                        if version == COMPATIBLE_AGENT_VERSION {
                            self.agent_state = AgentState::Ready(version)
                        }else {
                            self.agent_state = AgentState::AgentOutdated(version)
                        }
                    },
                    None => self.agent_state = AgentState::AgentMissing,
                }
            },
        }
    }
}