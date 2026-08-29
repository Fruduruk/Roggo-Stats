use eframe::egui;

use crate::core::ui::{
    components::full_panel::FullPanel,
};

#[derive(Default)]
pub struct MatchPage {}

impl MatchPage {
    pub fn ui(&mut self, ui: &mut egui::Ui) {
        FullPanel.show(ui, |ui| {
        });
    }
}
