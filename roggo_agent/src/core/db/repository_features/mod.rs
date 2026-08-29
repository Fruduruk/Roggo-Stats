pub mod day;

use rusqlite::types::Value;
use rusqlite::{params, params_from_iter};
use uuid::Uuid;
use crate::core::db::{Repository, Result};

use crate::core::bl::query_models::{
     GlobalPlayerRow,
};

impl Repository {
    pub fn get_player_with_most_replays(&self) -> Result<GlobalPlayerRow> {
        let mut stmt = self.connection.prepare(
            "
            select global_players.id,
                global_players.last_username,
                global_players.primary_id,
                count(players.global_player_id) as play_count
            from players
            join global_players on global_player_id = global_players.id
            group by global_player_id
            order by play_count desc
            limit 1
            ",
        )?;

        let row = stmt.query_row([], |row| {
            Ok(GlobalPlayerRow {
                id: row.get("id")?,
                primary_id: row.get("primary_id")?,
                last_username: row.get("last_username")?,
            })
        })?;

        Ok(row)
    }
}
