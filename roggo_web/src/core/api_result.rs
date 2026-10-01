pub enum APIResult {
    PlayerName(String),
    AgentError(roggo_contract::AgentErrorDto),
    GeneralError(crate::core::Error),
    Version(Option<String>),
    Day(roggo_contract::DayDto),
    DetailedSession(roggo_contract::SessionDto),
    DaysPlayed(Vec<jiff::civil::Date>),
}
