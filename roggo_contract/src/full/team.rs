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
        self.players.iter().find(|p| p.primary_id == primary_id)
    }

    pub fn mate_score_difference(&self) -> Option<f64> {
        let min = self.players.iter().map(|p| p.score).min()?;
        let max = self.players.iter().map(|p| p.score).max()?;

        if max == 0 {
            return Some(0.0);
        }
        
        let difference = max - min;
        Some(difference as f64 / max as f64)
    }
}
