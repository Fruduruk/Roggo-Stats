INSERT INTO teams (
    match_guid,
    name,
    team_num,
    score,
    color_primary,
    color_secondary
  )
VALUES (
    -- 'match_guid:uuid',
    ?1,
    -- 'name:TEXT',
    ?2,
    -- team_num:INTEGER,
    ?3,
    -- score:INTEGER,
    ?4,
    -- 'color_primary:TEXT',
    ?5,
    -- 'color_secondary:TEXT'
    ?6
  );