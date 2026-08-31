pub enum APIResult {
    PlayerName(String),
    AgentError(super::contract::AgentErrorDto),
    Version(Option<String>),
    Day(super::contract::DayDto),
    DetailedSession(super::contract::DetailedSessionDto),
}