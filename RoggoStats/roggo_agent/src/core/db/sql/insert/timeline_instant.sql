INSERT INTO timeline (
    match_guid,
    timestamp_ms,
    last_touch_team_id,
    ball_speed
  )
VALUES (
    ?1,
    ?2,
    ?3,
    ?4
  );