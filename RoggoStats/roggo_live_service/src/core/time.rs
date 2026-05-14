use chrono::{DateTime, Local};

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
