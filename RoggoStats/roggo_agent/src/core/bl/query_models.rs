use uuid::Uuid;


#[derive(Debug)]
pub struct MatchRow {
    pub match_guid: Uuid,
    pub arena: String,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub had_overtime: bool,
    pub deleted: bool
}

#[derive(Debug)]
pub struct PlayerPlayCountRow {
    pub last_username: String,
    pub primary_id: String,
    pub play_count: u32,
}