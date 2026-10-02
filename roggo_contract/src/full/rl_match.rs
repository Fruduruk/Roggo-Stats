use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::{
    Playlist,
    full::{Player, Team},
};

#[derive(Debug, Serialize, Deserialize)]
pub struct RLMatch {
    pub match_guid: Uuid,
    pub arena: String,
    pub duration: i64,
    pub created_at: i64,
    pub ended_at: i64,
    pub had_overtime: bool,
    pub deleted: bool,
    pub playlist: Playlist,
    pub teams: Vec<Team>,
}

impl RLMatch {
    pub fn get_player_by_primary_id(&self, primary_id: &str) -> Option<&Player> {
        self.teams
            .iter()
            .flat_map(|team| &team.players)
            .find(|player| player.primary_id == primary_id)
    }

    pub fn get_team_of_player_by_primary_id(&self, primary_id: &str) -> Option<&Team> {
        self.teams.iter().find(|t| {
            t.get_player_by_primary_id(primary_id).is_some()
        })
    }

    pub fn get_enemy_team_of_player_by_primary_id(&self, primary_id: &str) -> Option<&Team> {
        self.teams.iter().find(|t| {
            t.get_player_by_primary_id(primary_id).is_none()
        })
    }
}
