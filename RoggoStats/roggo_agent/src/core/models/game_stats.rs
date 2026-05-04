use crate::core::models::api_models::BallHit;

#[derive(Debug)]
pub struct GameStats {
    pub ball_hits: Vec<BallHit>,
}

impl Default for GameStats {
    fn default() -> Self {
        Self { ball_hits: vec![] }
    }
}
