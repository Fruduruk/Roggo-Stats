use std::sync::{Arc, Mutex};

use eframe::egui::{self, Context};
use uuid::Uuid;

use crate::core::{
    contract::{
        DetailedMatchDto, DetailedPlayerDto, DetailedPlayerStatsDto, DetailedSessionDto,
        DetailedTeamDto,
    },
    time::format_ms_min_seconds,
    ui::tasks,
};

#[derive(Default)]
pub struct Content {
    pub detailed_session: Option<DetailedSessionDto>,
}

#[derive(Default)]
pub struct SessionUi {
    content: Arc<Mutex<Content>>,
}
impl SessionUi {
    pub fn reload(&self, context: Context, match_guids: Vec<Uuid>) {
        tasks::load_detailed_session(context, self.content.clone(), match_guids);
    }

    pub fn ui(&self, ui: &mut egui::Ui) {
        let Ok(content) = self.content.lock() else {
            ui.label("Failed to lock match content.");
            return;
        };

        let Some(detailed_session) = &content.detailed_session else {
            ui.centered_and_justified(|ui| {
                ui.label("No session selected.");
            });
            return;
        };

        ui.centered_and_justified(|ui| {
            ui.label(format!("{:#?}", detailed_session.match_guids));
        });
    }
}
