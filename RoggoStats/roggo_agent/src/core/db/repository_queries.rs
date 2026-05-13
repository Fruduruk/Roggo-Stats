use crate::core::{bl::query_models::MatchRow};
use crate::core::db::{Error, Result,Repository};

impl Repository {
     pub fn get_match(&self) -> Result<Vec<MatchRow>> {
        let mut stmt = self.connection.prepare(
            "
            select * from matches
            order by created_at_ms desc
            ",
        )?;

        let rows = stmt.query_map([], |row| {
            Ok(MatchRow {
                match_guid: row.get("match_guid")?,
                arena: row.get("arena")?,
                duration: row.get("duration")?,
                created_at: row.get("created_at_ms")?,
                ended_at: row.get("ended_at_ms")?,
                had_overtime: row.get("had_overtime")?,
                deleted: row.get("deleted")?,
            })
        })?;

        let mut matches = vec![];

        for row in rows {
            matches.push(row?);
        }

        Ok(matches)
    }
}