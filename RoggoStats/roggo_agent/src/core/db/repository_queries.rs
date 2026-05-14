use rusqlite::params;

use crate::core::bl::query_models::{F2MatchRow, F2PlayerRow, F2TeamRow, GlobalPlayerRow};
use crate::core::db::{Repository, Result};

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

    pub fn f2_get_matches(&self) -> Result<Vec<F2MatchRow>> {
        let mut stmt = self.connection.prepare(
            "
            select * from matches
            where deleted = false
            ",
        )?;

        let rows = stmt.query_map([], |row| {
            Ok(F2MatchRow {
                id: row.get("id")?,
                match_guid: row.get("match_guid")?,
                duration: row.get("duration")?,
                ended_at: row.get("ended_at_ms")?,
            })
        })?;


        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn f2_get_teams_by_match_id(&self, match_id: i64) -> Result<Vec<F2TeamRow>> {
        let mut stmt = self.connection.prepare(
            "
            select teams.id, teams.score from teams
            where teams.match_id = ?1
            ",
        )?;

        let rows = stmt.query_map(params![match_id], |row| {
            Ok(F2TeamRow {
                id: row.get("id")?,
                score: row.get("score")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn f2_get_players_by_team_id(&self, team_id: i64) -> Result<Vec<F2PlayerRow>> {
        let mut stmt = self.connection.prepare(
            "
            select players.global_player_id from players
            where players.team_id = ?1
            ",
        )?;

        let rows = stmt.query_map(params![team_id], |row| {
            Ok(F2PlayerRow {
                global_player_id: row.get("global_player_id")?,
            })
        })?;


        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }
}
