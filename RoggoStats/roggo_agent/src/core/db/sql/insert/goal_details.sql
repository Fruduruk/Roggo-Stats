INSERT INTO goal_details (
    match_guid,
    timestamp_ms,
    goal_time,
    impact_x,
    impact_y,
    impact_z,
    goal_speed,
    last_touch_speed,
    scorer_player_id,
    assister_player_id,
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
    ?8,
    ?9,
    ?10,
    ?11
  );