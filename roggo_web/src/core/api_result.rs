pub enum APIResult {
    PlayerName(String),
    AgentError(roggo_contract::AgentErrorDto),
    GeneralError(crate::core::Error),
    Version(Option<String>),
    Day(roggo_contract::DayDto),
    Session(roggo_contract::SessionDto),
    SessionDetails(roggo_contract::SessionDetails),
    DaysPlayed(Vec<jiff::civil::Date>),
}
