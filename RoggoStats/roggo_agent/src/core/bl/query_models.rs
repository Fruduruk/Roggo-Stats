use uuid::Uuid;

pub struct F5AverageCoreStatsRow {
    pub average_score: f64,
    pub average_goals: f64,
    pub average_shots: f64,
    pub average_assists: f64,
    pub average_saves: f64,
    pub average_demos: f64,
}

pub struct F3PlayerStatsRow {
    pub time_boosting: i64,
    pub time_demolished: i64,
    pub time_on_ground: i64,
    pub time_on_wall: i64,
    pub time_powersliding: i64,
    pub time_supersonic: i64,
}

pub struct F3PlayerRow {
    pub id: i64,
    pub global_player_id: i64,
    pub display_name: String,
    pub score: i64,
    pub goals: i64,
    pub shots: i64,
    pub assists: i64,
    pub saves: i64,
    pub demos: i64,
    pub stats: Option<F3PlayerStatsRow>
}

pub struct F3TeamRow {
    pub id: i64,
    pub name: String,
    pub score: i64,
    pub color_primary: String,
    pub color_secondary: String,
}

pub struct F3MatchRow {
    pub id: i64,
    pub arena: String,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub had_overtime: bool,
}

pub struct F2PlayerRow {
    pub global_player_id: i64,
}

pub struct F2TeamRow {
    pub id: i64,
    pub score: i64,
}

pub struct F2MatchRow {
    pub id: i64,
    pub match_guid: Uuid,
    pub duration: i64,
    pub ended_at: i64,
}

pub struct F4MatchRow {
    pub id: i64,
    pub match_guid: Uuid,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
}

pub struct GlobalPlayerRow {
    pub id: i64,
    pub primary_id: String,
    pub last_username: String,
}
