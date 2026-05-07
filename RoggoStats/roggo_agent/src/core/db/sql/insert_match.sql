INSERT INTO matches (
        match_guid,
        arena,
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
        ?6
    );