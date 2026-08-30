use eframe::egui;

use crate::core::{contract::DayDto, ui::{components::full_panel::FullPanel, widgets::session_card}};

#[derive(Default)]
pub struct DayPage {}

impl DayPage {
    pub fn ui(&mut self, ui: &mut egui::Ui, day_dto: &DayDto) {
        FullPanel.show(ui, |ui| {
            for session in &day_dto.sessions {
                session_card::ui(ui, session);
            }
        });
    }
}
