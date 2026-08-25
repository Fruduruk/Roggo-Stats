#[derive(Default, PartialEq, Eq)]
pub enum AgentState {
    #[default]
    CheckingAgent,
    AgentMissing,
    AgentOutdated(String),
    Ready(String)
}