use eframe::egui;

use crate::core::ui::components::full_panel::FullPanel;

#[derive(Default)]
pub struct AllTimePage {}

impl AllTimePage {
    pub fn ui(&mut self, ui: &mut egui::Ui) {
        FullPanel.show(ui, |ui| {
            super::development_page::theme_edit(ui);
        });
    }
}
