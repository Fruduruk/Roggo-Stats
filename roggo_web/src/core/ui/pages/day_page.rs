use eframe::egui;

use crate::core::{
    contract::DayDto,
    ui::{components::full_panel::FullPanel, widgets::session_card},
};

#[derive(Default)]
pub struct DayPage {}

impl DayPage {
    pub fn ui(&mut self, ui: &mut egui::Ui, day_dto: &DayDto) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical().show(ui, |ui| {
                ui.columns(2, |columns| {
                    let left_column_ui = &mut columns[0];
                    self.show_session_cards(left_column_ui, day_dto);
                    let right_column_ui = &mut columns[1];
                    right_column_ui.heading("Day Stats");
                });
            });
        });
    }

    fn show_session_cards(&self, ui: &mut egui::Ui, day_dto: &DayDto) {
        for session in &day_dto.sessions {
            session_card::ui(ui, session);
        }
    }
}
