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
        self.teams
            .iter()
            .find(|t| t.get_player_by_primary_id(primary_id).is_some())
    }

    pub fn get_enemy_team_of_player_by_primary_id(&self, primary_id: &str) -> Option<&Team> {
        self.teams
            .iter()
            .find(|t| t.get_player_by_primary_id(primary_id).is_none())
    }

    pub fn won_by_player_primary_id(&self, primary_id: &str) -> Option<bool> {
        let players_team = self.get_team_of_player_by_primary_id(primary_id)?;
        let enemy_team = self.get_enemy_team_of_player_by_primary_id(primary_id)?;
        if players_team.score == enemy_team.score {
            return None;
        }
        Some(players_team.score > enemy_team.score)
    }

    pub fn primary_id_was_mvp(&self, primary_id: &str) -> Option<bool> {
        let players_team = self.get_team_of_player_by_primary_id(primary_id)?;
        let mvp = players_team
            .players
            .iter()
            .max_by(|p, p2| p.score.cmp(&p2.score))?;
        Some(mvp.primary_id == primary_id)
    }
}
