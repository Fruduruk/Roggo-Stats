use crate::core::{api::dto::MatchDto, bl::query_models::MatchRow};


impl From<MatchRow> for MatchDto {
    fn from(value: MatchRow) -> Self {
        MatchDto {
            match_guid: value.match_guid,
            arena: value.arena,
            duration: value.duration,
            created_at: value.created_at,
            ended_at: value.ended_at,
            had_overtime: value.had_overtime,
            deleted: value.deleted,
        }
    }
}
