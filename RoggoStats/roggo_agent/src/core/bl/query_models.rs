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