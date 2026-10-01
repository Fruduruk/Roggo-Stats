use crate::core::db::{Repository, Result, repository_features::create_value_placeholders};
use roggo_contract::*;
use rusqlite::params_from_iter;
use uuid::Uuid;

#[derive(Debug)]
pub struct MatchRow {
    pub id: i64,
    pub match_guid: Uuid,
    pub arena: String,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub had_overtime: bool,
    pub deleted: bool,
    pub playlist: Playlist,
}

#[derive(Debug)]
pub struct TeamRow {
    pub match_id: i64,
    pub id: i64,
    pub team_num: i64,
    pub name: String,
    pub score: i64,
    pub color_primary: String,
    pub color_secondary: String,
}

#[derive(Debug)]
pub struct PlayerRow {
    pub id: i64,
    pub team_id: i64,

    pub primary_id: String,
    pub last_username: String,
    pub display_name: String,

    pub shortcut: i64,
    pub score: i64,
    pub goals: i64,
    pub shots: i64,
    pub assists: i64,
    pub saves: i64,
    pub touches: i64,
    pub car_touches: i64,
    pub demos: i64,
}

#[derive(Debug)]
pub struct PlayerStatsRow {
    pub player_id: i64,

    pub percent_boosting: f64,
    pub percent_demolished: f64,
    pub percent_on_ground: f64,
    pub percent_on_wall: f64,
    pub percent_powersliding: f64,
    pub percent_supersonic: f64,
}

#[derive(Debug)]
pub struct StatfeedEventRow {
    pub main_target_player_id: i64,

    pub timestamp: f32,
    pub event_name: String,
    pub event_type: String,

    pub secondary_target_primary_id: Option<String>,
}

impl Repository {
    pub fn get_match_rows_by_match_guids(&self, match_guids: Vec<Uuid>) -> Result<Vec<MatchRow>> {
        let (mut stmt, params) = self.prepare_statement_and_params_for_match_guids(
            match_guids,
            "
                select m.* from matches m
                join selected_matches sm on m.match_guid = sm.match_guid
                where duration != 0
                and deleted == 0
            ",
        )?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            let playlist_id: u32 = row.get("playlist")?;

            Ok(MatchRow {
                id: row.get("id")?,
                match_guid: row.get("match_guid")?,
                duration: row.get("duration")?,
                created_at: row.get("created_at_ms")?,
                ended_at: row.get("ended_at_ms")?,
                arena: row.get("arena")?,
                had_overtime: row.get("had_overtime")?,
                deleted: row.get("deleted")?,
                playlist: playlist_id.into(),
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn get_team_rows_by_match_ids(&self, match_ids: Vec<i64>) -> Result<Vec<TeamRow>> {
        let (placeholders, params) =
            create_value_placeholders(match_ids, 1, rusqlite::types::Value::Integer);

        let mut stmt = self.connection.prepare(&format!(
            "
                select t.* from teams t
                where t.match_id in {}
            ",
            placeholders
        ))?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            Ok(TeamRow {
                match_id: row.get("match_id")?,
                id: row.get("id")?,
                score: row.get("score")?,
                team_num: row.get("team_num")?,
                name: row.get("name")?,
                color_primary: row.get("color_primary")?,
                color_secondary: row.get("color_secondary")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn get_player_rows_by_team_ids(&self, team_ids: Vec<i64>) -> Result<Vec<PlayerRow>> {
        let (placeholders, params) =
            create_value_placeholders(team_ids, 1, rusqlite::types::Value::Integer);

        let mut stmt = self.connection.prepare(&format!(
            "
                select
                    p.*,
                    gp.primary_id,
                    gp.last_username
                from players p
                join global_players gp on gp.id = p.global_player_id
                where p.team_id in {}
            ",
            placeholders
        ))?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            Ok(PlayerRow {
                id: row.get("id")?,
                team_id: row.get("team_id")?,

                primary_id: row.get("primary_id")?,
                last_username: row.get("last_username")?,

                display_name: row.get("display_name")?,
                shortcut: row.get("shortcut")?,
                score: row.get("score")?,
                goals: row.get("goals")?,
                shots: row.get("shots")?,
                assists: row.get("assists")?,
                saves: row.get("saves")?,
                touches: row.get("touches")?,
                car_touches: row.get("car_touches")?,
                demos: row.get("demos")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn get_player_stats_rows_by_player_ids(
        &self,
        player_ids: Vec<i64>,
    ) -> Result<Vec<PlayerStatsRow>> {
        let (placeholders, params) =
            create_value_placeholders(player_ids, 1, rusqlite::types::Value::Integer);

        let mut stmt = self.connection.prepare(&format!(
            "
            select
                ps.player_id,
                1.0 * ps.time_boosting / m.duration as percent_boosting,
                1.0 * ps.time_demolished / m.duration as percent_demolished,
                1.0 * ps.time_on_ground / m.duration as percent_on_ground,
                1.0 * ps.time_on_wall / m.duration as percent_on_wall,
                1.0 * ps.time_powersliding / m.duration as percent_powersliding,
                1.0 * ps.time_supersonic / m.duration as percent_supersonic
            from player_stats ps
            join players p
                on p.id = ps.player_id
            join matches m
                on m.id = p.match_id
            where ps.player_id in {}
        ",
            placeholders
        ))?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            Ok(PlayerStatsRow {
                player_id: row.get("player_id")?,
                percent_boosting: row.get("percent_boosting")?,
                percent_demolished: row.get("percent_demolished")?,
                percent_on_ground: row.get("percent_on_ground")?,
                percent_on_wall: row.get("percent_on_wall")?,
                percent_powersliding: row.get("percent_powersliding")?,
                percent_supersonic: row.get("percent_supersonic")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }

    pub fn get_statfeed_event_rows_by_player_ids(
        &self,
        player_ids: Vec<i64>,
    ) -> Result<Vec<StatfeedEventRow>> {
        let (placeholders, params) =
            create_value_placeholders(player_ids, 1, rusqlite::types::Value::Integer);

        let mut stmt = self.connection.prepare(&format!(
            "
                select
                    sfe.main_target_player_id,
                    sfe.timestamp_ms,
                    sfe.event_name,
                    sfe.event_type,
                    secondary_gp.primary_id as secondary_target_primary_id
                from statfeed_events sfe
                left join players secondary_player
                    on secondary_player.id = sfe.secondary_target_player_id
                left join global_players secondary_gp
                    on secondary_gp.id = secondary_player.global_player_id
                where sfe.main_target_player_id in {}
            ",
            placeholders
        ))?;

        let rows = stmt.query_map(params_from_iter(params.iter()), |row| {
            Ok(StatfeedEventRow {
                main_target_player_id: row.get("main_target_player_id")?,
                timestamp: row.get("timestamp_ms")?,
                event_name: row.get("event_name")?,
                event_type: row.get("event_type")?,
                secondary_target_primary_id: row.get("secondary_target_primary_id")?,
            })
        })?;

        Ok(rows.collect::<rusqlite::Result<Vec<_>>>()?)
    }
}
