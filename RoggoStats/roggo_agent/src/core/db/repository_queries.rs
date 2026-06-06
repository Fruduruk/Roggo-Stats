use rusqlite::params;
use uuid::Uuid;

use crate::core::bl::query_models::{
    F2MatchRow, F2PlayerRow, F2TeamRow, F3MatchRow, F3PlayerRow, F3PlayerStatsRow, F3TeamRow,
    GlobalPlayerRow,
};
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
            and duration != 0
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

    pub fn f3_get_match_by_match_guid(&self, match_guid: Uuid) -> Result<F3MatchRow> {
        let mut stmt = self.connection.prepare(
            "
            select * from matches
            where matches.match_guid = ?1
            limit 1
            ",
        )?;

        let row = stmt.query_row(params![match_guid], |row| {
            Ok(F3MatchRow {
                id: row.get("id")?,
                arena: row.get("arena")?,
                duration: row.get("duration")?,
                created_at: row.get("created_at_ms")?,
                ended_at: row.get("ended_at_ms")?,
                had_overtime: row.get("had_overtime")?,
            })
        })?;

        Ok(row)
    }

    pub fn f3_get_teams_by_match_id(&self, match_id: i64) -> Result<Vec<F3TeamRow>> {
        let mut stmt = self.connection.prepare(
            "
            select 
                teams.id,
                teams.score,
                teams.name,
                teams.color_primary,
                teams.color_secondary
            from teams
            where teams.match_id = ?1
            ",
        )?;

        let rows = stmt.query_map(params![match_id], |row| {
            Ok(F3TeamRow {
                id: row.get("id")?,
                score: row.get("score")?,
                name: row.get("name")?,
                color_primary: row.get("color_primary")?,
                color_secondary: row.get("color_secondary")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn f3_get_players_by_team_id(&self, team_id: i64) -> Result<Vec<F3PlayerRow>> {
        let mut stmt = self.connection.prepare(
            "
            select
                p.id,
                p.global_player_id,
                p.display_name,
                p.score,
                p.goals,
                p.shots,
                p.assists,
                p.saves,
                p.touches,
                p.car_touches,
                p.demos,

                ps.player_id as stats_player_id,

                ps.time_boosting,
                ps.time_demolished,
                ps.time_on_ground,
                ps.time_on_wall,
                ps.time_powersliding,
                ps.time_supersonic
            from players p
            left join player_stats ps on ps.player_id = p.id
            where p.team_id = ?1
            ",
        )?;

        let rows = stmt.query_map(params![team_id], |row| {
            let stats_player_id: Option<i64> = row.get("stats_player_id")?;

            let stats = match stats_player_id.is_some() {
                true => Some(F3PlayerStatsRow {
                    time_boosting: row.get("time_boosting")?,
                    time_demolished: row.get("time_demolished")?,
                    time_on_ground: row.get("time_on_ground")?,
                    time_on_wall: row.get("time_on_wall")?,
                    time_powersliding: row.get("time_powersliding")?,
                    time_supersonic: row.get("time_supersonic")?,
                }),
                false => None,
            };

            Ok(F3PlayerRow {
                global_player_id: row.get("global_player_id")?,
                id: row.get("id")?,
                display_name: row.get("display_name")?,
                score: row.get("score")?,
                goals: row.get("goals")?,
                shots: row.get("shots")?,
                assists: row.get("assists")?,
                saves: row.get("saves")?,
                touches: row.get("touches")?,
                car_touches: row.get("car_touches")?,
                demos: row.get("demos")?,
                stats,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn get_global_player_by_id(&self, id: i64) -> Result<GlobalPlayerRow> {
        let mut stmt = self.connection.prepare(
            "
            select * from global_players
            where id = ?1
            "
        )?;


         let row = stmt.query_row(params![id], |row| {
            Ok(GlobalPlayerRow {
                id: row.get("id")?,
                primary_id: row.get("primary_id")?,
                last_username: row.get("last_username")?,
            })
        })?;

        Ok(row)
    }
}
