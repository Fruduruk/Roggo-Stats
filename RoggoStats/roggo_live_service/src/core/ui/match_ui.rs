use std::sync::{Arc, Mutex};

use crate::core::contract::DetailedMatchDto;

#[derive(Default)]
pub struct Content {
    pub detailed_match: Option<DetailedMatchDto>,
}

#[derive(Default)]
pub struct MatchUi {
    content: Arc<Mutex<Content>>,
}
