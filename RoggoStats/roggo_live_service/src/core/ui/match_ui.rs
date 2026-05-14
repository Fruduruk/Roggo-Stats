use std::sync::{Arc, Mutex};

use uuid::Uuid;

use crate::core::{contract::DetailedMatchDto, ui::tasks};

#[derive(Default)]
pub struct Content {
    pub detailed_match: Option<DetailedMatchDto>,
}

#[derive(Default)]
pub struct MatchUi {
    content: Arc<Mutex<Content>>,
}

impl MatchUi {
    pub fn reload(&self, match_guid: Uuid) {
        tasks::load_detailed_match_by_id(self.content.clone(), match_guid);
    }

    pub fn ui(&self, ui: &mut eframe::egui::Ui) {
        if let Ok(content) = self.content.lock() {
            if let Some(detailed_match) = &content.detailed_match {
                let header = format!("{} vs {}", detailed_match.own_team.name, detailed_match.enemy_team.name);
                
                ui.heading(&header);

            }
        }
    }
}
