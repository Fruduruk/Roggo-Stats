use eframe::egui;

use crate::core::{contract::DetailedSessionDto, ui::components::full_panel::FullPanel};

#[derive(Default)]
pub struct SessionPage {}

impl SessionPage {
    pub fn ui(&mut self, ui: &mut egui::Ui, detailed_session: &DetailedSessionDto) {
        FullPanel.show(ui, |ui| {
            for m in &detailed_session.session_matches {
                ui.label(m.match_guid.to_string());
            }
        });
    }
}
