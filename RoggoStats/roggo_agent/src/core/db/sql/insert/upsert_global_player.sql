INSERT INTO global_players (primary_id, username)
VALUES (?1, ?2)
ON CONFLICT(primary_id) DO UPDATE SET
    username = excluded.username;