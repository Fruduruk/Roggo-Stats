use serde::{Deserialize, Serialize};

use crate::full::Player;

#[derive(Debug, Serialize, Deserialize)]
pub struct Team {
    pub team_num: i64,
    pub name: String,
    pub score: i64,
    pub color_primary: String,
    pub color_secondary: String,
    pub players: Vec<Player>,
}

impl Team {
    pub fn get_player_by_primary_id(&self, primary_id: &str) -> Option<&Player> {
        self.players.iter().find(|p| {
            p.primary_id == primary_id
        })  
    }
}
