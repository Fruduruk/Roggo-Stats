INSERT INTO statfeed_events (
    match_guid,
    timestamp_ms,
    event_name,
    event_type,
    main_target_player_id,
    secondary_target_player_id
  )
VALUES (
    ?1,
    ?2,
    ?3,
    ?4,
    ?5,
    ?6
  );