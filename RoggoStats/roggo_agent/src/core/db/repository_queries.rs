use rusqlite::types::Value;
use rusqlite::{params, params_from_iter};
use uuid::Uuid;

use crate::core::bl::query_models::{
    F2MatchRow, F2PlayerRow, F2TeamRow, F3MatchRow, F3PlayerRow, F3PlayerStatsRow, F3TeamRow,
    F4MatchRow, F5AverageAdvancedStatsRow, F5AverageCoreStatsRow, F5AveragePlayerStatsRow,
    F5SessionMatchRow, GlobalPlayerRow,
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
            ",
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

    pub fn f4_get_matches(&self) -> Result<Vec<F4MatchRow>> {
        let mut stmt = self.connection.prepare(
            "
            select
                id,
                match_guid,
                duration,
                ended_at_ms,
                created_at_ms
            from matches
            where deleted = false
            and duration != 0
            order by ended_at_ms asc
            ",
        )?;

        let rows = stmt.query_map([], |row| {
            Ok(F4MatchRow {
                id: row.get("id")?,
                match_guid: row.get("match_guid")?,
                duration: row.get("duration")?,
                ended_at: row.get("ended_at_ms")?,
                created_at: row.get("created_at_ms")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn f5_get_own_team_player_averages(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
    ) -> Result<Vec<F5AveragePlayerStatsRow>> {
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

        let rest = include_str!("sql/average_stats_for_player_and_friends.sql");

        let mut stmt = self.connection.prepare(&format!("{start}{rest}"))?;

        let mut params = vec![Value::Integer(main_character_global_player_id)];
        params.extend(
            match_guids
                .into_iter()
                .map(|guid| Value::Blob(guid.as_bytes().to_vec())),
        );

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            Ok(F5AveragePlayerStatsRow {
                global_player_id: row.get("global_player_id")?,
                last_username: row.get("last_username")?,

                average_core_stats: F5AverageCoreStatsRow {
                    average_score: row.get("average_score")?,
                    average_goals: row.get("average_goals")?,
                    average_shots: row.get("average_shots")?,
                    average_assists: row.get("average_assists")?,
                    average_saves: row.get("average_saves")?,
                    average_demos: row.get("average_demos")?,
                },
                average_advanced_stats: F5AverageAdvancedStatsRow {
                    average_percent_boosting: row.get("average_percent_boosting")?,
                    average_percent_demolished: row.get("average_percent_demolished")?,
                    average_percent_on_ground: row.get("average_percent_on_ground")?,
                    average_percent_on_wall: row.get("average_percent_on_wall")?,
                    average_percent_powersliding: row.get("average_percent_powersliding")?,
                    average_percent_supersonic: row.get("average_percent_supersonic")?,
                },
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn f5_get_enemy_player_core_averages(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
    ) -> Result<F5AverageCoreStatsRow> {
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

        let rest = include_str!("sql/average_enemy_core_stats.sql");

        let mut stmt = self.connection.prepare(&format!("{start}{rest}"))?;

        let mut params = vec![Value::Integer(main_character_global_player_id)];
        params.extend(
            match_guids
                .into_iter()
                .map(|guid| Value::Blob(guid.as_bytes().to_vec())),
        );

        let row = stmt.query_row(params_from_iter(params.iter()), |row| {
            Ok(F5AverageCoreStatsRow {
                average_score: row.get("average_score")?,
                average_goals: row.get("average_goals")?,
                average_shots: row.get("average_shots")?,
                average_assists: row.get("average_assists")?,
                average_saves: row.get("average_saves")?,
                average_demos: row.get("average_demos")?,
            })
        })?;

        Ok(row)
    }

    pub fn f5_get_team_player_core_averages(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
    ) -> Result<F5AverageCoreStatsRow> {
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

        let rest = include_str!("sql/average_team_player_core_stats.sql");

        let mut stmt = self.connection.prepare(&format!("{start}{rest}"))?;

        let mut params = vec![Value::Integer(main_character_global_player_id)];
        params.extend(
            match_guids
                .into_iter()
                .map(|guid| Value::Blob(guid.as_bytes().to_vec())),
        );

        let row = stmt.query_row(params_from_iter(params.iter()), |row| {
            Ok(F5AverageCoreStatsRow {
                average_score: row.get("average_score")?,
                average_goals: row.get("average_goals")?,
                average_shots: row.get("average_shots")?,
                average_assists: row.get("average_assists")?,
                average_saves: row.get("average_saves")?,
                average_demos: row.get("average_demos")?,
            })
        })?;

        Ok(row)
    }

    pub fn f5_get_session_matches(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
    ) -> Result<Vec<F5SessionMatchRow>> {
        let (mut stmt, params) = self
            .prepare_statement_and_params_for_match_guids_and_main_character(
                match_guids,
                main_character_global_player_id,
                include_str!("sql/session_matches.sql"),
            )?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            Ok(F5SessionMatchRow {
                match_guid: row.get("match_guid")?,
                created_at: row.get("created_at_ms")?,
                ended_at: row.get("ended_at_ms")?,
                hidden: row.get("deleted")?,
                main_character_won: row.get("main_character_won")?,
                own_best_global_player_id: row.get("own_best_global_player_id")?,
                own_best_score: row.get("own_best_score")?,
                enemy_best_global_player_id: row.get("enemy_best_global_player_id")?,
                enemy_best_score: row.get("enemy_best_score")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
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

    pub fn f5_get_team_player_advanced_averages(
        &self,
        match_guids: Vec<Uuid>,
        main_character_global_player_id: i64,
    ) -> Result<F5AverageAdvancedStatsRow> {
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

        let rest = include_str!("sql/average_team_player_advanced_stats.sql");

        let mut stmt = self.connection.prepare(&format!("{start}{rest}"))?;

        let mut params = vec![Value::Integer(main_character_global_player_id)];
        params.extend(
            match_guids
                .into_iter()
                .map(|guid| Value::Blob(guid.as_bytes().to_vec())),
        );

        let row = stmt.query_row(params_from_iter(params.iter()), |row| {
            Ok(F5AverageAdvancedStatsRow {
                average_percent_boosting: row.get("average_percent_boosting")?,
                average_percent_demolished: row.get("average_percent_demolished")?,
                average_percent_on_ground: row.get("average_percent_on_ground")?,
                average_percent_on_wall: row.get("average_percent_on_wall")?,
                average_percent_powersliding: row.get("average_percent_powersliding")?,
                average_percent_supersonic: row.get("average_percent_supersonic")?,
            })
        })?;

        Ok(row)
    }

    pub fn hide_match(&self, match_guid: uuid::Uuid, hide: bool) -> Result<()> {
        let mut stmt = self.connection.prepare(
            "
            update matches
            set deleted = ?2
            where match_guid = ?1
            ",
        )?;

        stmt.execute(params![match_guid, hide])?;

        Ok(())
    }
}
