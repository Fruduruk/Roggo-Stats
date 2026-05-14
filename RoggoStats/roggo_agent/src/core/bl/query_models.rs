use uuid::Uuid;

#[derive(Debug)]
pub struct F2PlayerRow {
    pub global_player_id: i64,
}

#[derive(Debug)]
pub struct F2TeamRow {
    pub id: i64,
    pub score: i64,
}

#[derive(Debug)]
pub struct F2MatchRow {
    pub id: i64,
    pub match_guid: Uuid,
    pub duration: i64,
    pub ended_at: i64,
}

#[derive(Debug)]
pub struct GlobalPlayerRow {
    pub id: i64,
    pub primary_id: String,
    pub last_username: String,
}
