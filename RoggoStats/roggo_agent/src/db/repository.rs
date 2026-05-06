use std::path::Path;

use rusqlite::{Connection, Result, params};

use crate::core::models::game_stats::{GameStats, PlayerStats};

pub struct Repository {
    connection: Connection,
}

impl Repository {
    pub fn new(path: impl AsRef<Path>) -> Result<Self> {
        let repo = Self {
            connection: Connection::open(path)?,
        };
        repo.init()?;

        Ok(repo)
    }

    pub fn new_in_memory() -> Result<Self> {
        let repo = Self {
            connection: Connection::open_in_memory()?,
        };
        repo.init()?;

        Ok(repo)
    }

    fn init(&self) -> Result<()> {
        self.connection.pragma_update(None, "journal_mode", "WAL")?;
        self.connection.pragma_update(None, "busy_timeout", 5000)?;
        self.connection.pragma_update(None,"foreign_keys", "ON")?;

        self.connection
            .execute_batch(include_str!("sql/init.sql"))?;
        Ok(())
    }

    pub fn insert_match(&mut self, stats: GameStats) -> Result<()> {
        // let match_guid = stats.match_guid;
        // let tx = self.connection.transaction()?;
        // tx.execute(
        //     include_str!("sql/insert_match.sql"),
        //     params![
        //         match_guid,
        //         stats.arena_name,
        //         stats.created_at_timestamp,
        //         stats.ended_at_timestamp,
        //         stats.had_overtime,
        //         false
        //     ],
        // )?;

        // for (team_num, team) in stats.teams {
        //     tx.execute(
        //         include_str!("sql/insert_team.sql"),
        //         params![
        //             match_guid,
        //             team.name.clone(),
        //             team_num,
        //             team.score,
        //             team.color_primary,
        //             team.color_secondary
        //         ],
        //     )?;

        //     for (player_name, player) in team.players {
        //         if player.primary_id.contains("Unknown") { // Ignore Bots
        //             continue;
        //         }

        //         tx.execute(
        //             include_str!("sql/upsert_player.sql"),
        //             params![player.primary_id, player_name],
        //         )?;

        //         tx.execute(
        //             include_str!("sql/insert_player_stats.sql"),
        //             params![
        //                 match_guid,
        //                 team_num,
        //                 player.primary_id,
        //                 player.name,
        //                 player.shortcut,
        //                 player.score,
        //                 player.goals,
        //                 player.shots,
        //                 player.assists,
        //                 player.saves,
        //                 player.touches,
        //                 player.car_touches,
        //                 player.demos,
        //                 player.time_boosting,
        //                 player.time_demolished,
        //                 player.time_on_ground,
        //                 player.time_on_wall,
        //                 player.time_powersliding,
        //                 player.time_supersonic
        //             ],
        //         )?;
        //     }
        // }

        // tx.commit()?;
        Ok(())
    }
}
