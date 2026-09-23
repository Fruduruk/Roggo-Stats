use crate::core::api::contract::Playlist;
use crate::core::db::{Repository, Result};
use rusqlite::{params_from_iter};
use uuid::Uuid;

#[derive(Debug)]
pub struct PlayerRow {
    pub match_guid: Uuid,
    pub primary_id: String,
    pub display_name: String,
}

#[derive(Debug)]
pub struct SessionMatchRow {
    pub id: i64,
    pub match_guid: Uuid,
    pub arena: String,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub had_overtime: bool,
    pub deleted: bool,
    pub playlist: Playlist,

    pub own_score: i64,
    pub enemy_score: i64,
    pub main_character_won: Option<bool>,
    pub own_best_global_player_id: i64,
}

impl Repository {
    pub fn get_session_matches(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
    ) -> Result<Vec<SessionMatchRow>> {
        let (mut stmt, params) = self
            .prepare_statement_and_params_for_match_guids_and_main_character(
                match_guids,
                main_character_global_player_id,
                include_str!("../sql/session_matches.sql"),
            )?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            let playlist_id: u32 = row.get("playlist")?;

            Ok(SessionMatchRow {
                id: row.get("id")?,
                match_guid: row.get("match_guid")?,
                arena: row.get("arena")?,
                duration: row.get("duration")?,
                created_at: row.get("created_at_ms")?,
                ended_at: row.get("ended_at_ms")?,
                had_overtime: row.get("had_overtime")?,
                deleted: row.get("deleted")?,
                playlist: playlist_id.into(),

                main_character_won: row.get("main_character_won")?,
                own_best_global_player_id: row.get("own_best_global_player_id")?,
                own_score: row.get("own_score")?,
                enemy_score: row.get("enemy_score")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn get_session_enemies(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
    ) -> Result<Vec<PlayerRow>> {
        let (mut stmt, params) = self
            .prepare_statement_and_params_for_match_guids_and_main_character(
                match_guids,
                main_character_global_player_id,
                include_str!("../sql/session_enemies.sql"),
            )?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            Ok(PlayerRow {
                match_guid: row.get("match_guid")?,
                primary_id: row.get("primary_id")?,
                display_name: row.get("last_username")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }
}
