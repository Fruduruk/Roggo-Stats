use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::full::RLMatch;

#[derive(Debug, Serialize, Deserialize)]
pub struct SessionDetailsRequest {
    pub match_guids: Vec<Uuid>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct SessionDetails {
    pub matches: Vec<RLMatch>
}
