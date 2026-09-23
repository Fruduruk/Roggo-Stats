pub mod day;
pub mod session;

use rusqlite::types::Value;
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

    fn prepare_statement_and_params_for_match_guids_and_main_character(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
        rest: &str,
    ) -> Result<(rusqlite::Statement<'_>, Vec<Value>)> {
        let values_placeholders = (2..=match_guids.len() + 1) // Start at 2, because player_id is 1
            .map(|i| format!("(?{i})"))
            .collect::<Vec<_>>()
            .join(",");
        let start = format!(
            "
                with selected_matches(match_guid) as (
                values
                    {values_placeholders}
                ),
            "
        );
        let stmt = self.connection.prepare(&format!("{start}{rest}"))?;
        let mut params = vec![Value::Integer(main_character_global_player_id)];
        params.extend(
            match_guids
                .into_iter()
                .map(|guid| Value::Blob(guid.as_bytes().to_vec())),
        );
        Ok((stmt, params))
    }
}
