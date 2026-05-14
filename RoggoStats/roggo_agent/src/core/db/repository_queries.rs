use crate::core::bl::query_models::PlayerPlayCountRow;
use crate::core::{bl::query_models::MatchRow};
use crate::core::db::{Result,Repository};

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

    pub fn get_player_play_count(&self) -> Result<Vec<PlayerPlayCountRow>> {
        let mut stmt = self.connection.prepare(
            "
            select 
                global_players.last_username,
                global_players.primary_id,
                count(players.global_player_id) as play_count 
            from players
            join global_players on global_player_id = global_players.id
            group by global_player_id
            "
        )?;

        let rows = stmt.query_map([], |row| {
            Ok(PlayerPlayCountRow {
                last_username: row.get("last_username")?,
                primary_id: row.get("primary_id")?,
                play_count: row.get("play_count")?,
            })
        })?;

        let mut play_counts = vec![];
        for row in rows {
            play_counts.push(row?);
        }
        Ok(play_counts)
    }
}