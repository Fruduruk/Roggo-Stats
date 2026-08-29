use crate::core::api::contract::Playlist;
use crate::core::db::{Repository, Result};
use rusqlite::{params, params_from_iter};
use uuid::Uuid;

#[derive(Debug)]
pub struct DayMatchRow {
    pub id: i64,
    pub match_guid: Uuid,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub playlist: Playlist,
}

pub struct TeamRow {
    pub id: i64,
    pub score: i64,
}

pub struct PlayerRow {
    pub global_player_id: i64,
    pub display_name: String,
    pub primary_id: String,
}

impl Repository {
    pub fn get_all_matches_for_day(&self, start_ms: i64, end_ms: i64) -> Result<Vec<DayMatchRow>> {
        let mut stmt = self.connection.prepare(
            "
            select * from matches
            where duration != 0
            and deleted == 0
            and created_at_ms >= ?1
            and created_at_ms < ?2
            ",
        )?;

        let rows = stmt.query_map([start_ms, end_ms], |row| {
            let playlist_id: u32 = row.get("playlist")?;
            Ok(DayMatchRow {
                id: row.get("id")?,
                match_guid: row.get("match_guid")?,
                duration: row.get("duration")?,
                created_at: row.get("created_at_ms")?,
                ended_at: row.get("ended_at_ms")?,
                playlist: playlist_id.into(),
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }
}

pub fn get_teams_by_match_id(repo: &Repository, match_id: i64) -> Result<Vec<TeamRow>> {
    let mut stmt = repo.connection.prepare(
        "
            select teams.id, teams.score from teams
            where teams.match_id = ?1
            ",
    )?;

    let rows = stmt.query_map(params![match_id], |row| {
        Ok(TeamRow {
            id: row.get("id")?,
            score: row.get("score")?,
        })
    })?;

    Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
}

pub fn get_players_by_team_id(repo: &Repository, team_id: i64) -> Result<Vec<PlayerRow>> {
    let mut stmt = repo.connection.prepare(
        "
            select 
            p.global_player_id,
            gp.last_username,
            gp.primary_id
            from players p
                join global_players gp on p.global_player_id = gp.id
            where p.team_id = ?1
            ",
    )?;

    let rows = stmt.query_map(params![team_id], |row| {
        Ok(PlayerRow {
            global_player_id: row.get("global_player_id")?,
            display_name: row.get("last_username")?,
            primary_id: row.get("primary_id")?,
        })
    })?;

    Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
}
