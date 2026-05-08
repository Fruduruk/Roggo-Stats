INSERT INTO clock_samples (
    match_guid,
    timestamp_ms,
    time_seconds,
    is_overtime
  )
VALUES (
    ?1,
    ?2,
    ?3,
    ?4
  );