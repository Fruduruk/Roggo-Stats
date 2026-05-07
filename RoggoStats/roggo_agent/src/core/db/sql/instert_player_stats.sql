INSERT INTO player_stats (
    player_id,
    time_boosting,
    time_demolished,
    time_on_ground,
    time_on_wall,
    time_powersliding,
    time_supersonic
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