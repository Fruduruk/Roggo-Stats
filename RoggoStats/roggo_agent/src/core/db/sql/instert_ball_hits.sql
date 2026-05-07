INSERT INTO ball_hits (
    match_guid,
    timestamp_ms,
    pre_hit_speed,
    post_hit_speed,
    x,
    y,
    z
  )
VALUES (
    ?1,
    ?2,
    ?3,
    ?4,
    ?5,
    ?6,
    ?7
  );