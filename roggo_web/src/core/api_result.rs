pub enum APIResult {
    PlayerName(String),
    AgentError(super::contract::AgentErrorDto),
    Version(Option<String>),
}