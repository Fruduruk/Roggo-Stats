INSERT INTO crossbar_hits (
    match_guid,
    timestamp_ms,
    ball_speed,
    impact_force,
    x,
    y,
    z,
    last_touch_player_id
  )
VALUES (
   ?1,
   ?2,
   ?3,
   ?4,
   ?5,
   ?6,
   ?7,
   ?8
  );