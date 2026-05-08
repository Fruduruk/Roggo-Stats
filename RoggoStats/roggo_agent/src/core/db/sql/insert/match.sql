INSERT INTO matches (
        match_guid,
        arena,
        duration,
        created_at_ms,
        ended_at_ms,
        had_overtime,
        deleted
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