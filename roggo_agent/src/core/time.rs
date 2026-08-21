use crate::core::{Error, Result};
use chrono::{DateTime, Local};
use std::time::{SystemTime, UNIX_EPOCH};

pub fn format_ms_date(timestamp_ms: i64) -> String {
    DateTime::from_timestamp_millis(timestamp_ms)
        .map(|dt| dt.with_timezone(&Local))
        .map(|dt| dt.format("%d.%m.%Y").to_string())
        .unwrap_or_else(|| "Invalid date".to_string())
}

pub fn format_ms_time(timestamp_ms: i64) -> String {
    DateTime::from_timestamp_millis(timestamp_ms)
        .map(|dt| dt.with_timezone(&Local))
        .map(|dt| dt.format("%H:%M:%S").to_string())
        .unwrap_or_else(|| "Invalid time".to_string())
}

pub fn format_ms_min_seconds(duration_ms: i64) -> String {
    let total_seconds = duration_ms / 1000;
    let minutes = total_seconds / 60;
    let seconds = total_seconds % 60;

    format!("{minutes}:{seconds:02}")
}

pub fn format_ms_to_date_time(timestamp_ms: i64) -> String {
    DateTime::from_timestamp_millis(timestamp_ms)
        .map(|dt| dt.with_timezone(&Local))
        .map(|dt| dt.format("%Y-%m-%d_%H-%M-%S").to_string())
        .unwrap_or_else(|| "invalid-date-time".to_string())
}

#[inline]
pub fn now() -> Result<i64> {
    i64::try_from(
        SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .map_err(|_| Error::GeneralError("System time is before UNIX_EPOCH".into()))?
            .as_millis(),
    )
    .map_err(|_| Error::GeneralError("System time millis does not fit into i64".into()))
}
