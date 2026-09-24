pub enum APIResult {
    PlayerName(String),
    AgentError(super::contract::AgentErrorDto),
    GeneralError(crate::core::Error),
    Version(Option<String>),
    Day(super::contract::day::DayDto),
    DetailedSession(super::contract::session::SessionDto),
    DaysPlayed(Vec<jiff::civil::Date>),
}
