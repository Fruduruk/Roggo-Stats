INSERT INTO matches (
        match_guid,
        arena,
        created_at_ms,
        ended_at_ms,
        had_overtime
    )
VALUES (
        -- 'match_guid:uuid',
        ?1,
        -- 'arena:TEXT',
        ?2,
        -- created_at_ms:INTEGER,
        ?3,
        -- ended_at_ms:INTEGER,
        ?4,
        -- had_overtime:boolean
        ?5
    );