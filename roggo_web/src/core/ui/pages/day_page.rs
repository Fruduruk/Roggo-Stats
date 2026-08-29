use eframe::egui;
use jiff::civil::Date;

use crate::core::ui::{
    components::full_panel::FullPanel,
};

#[derive(Default)]
pub struct DayPage {}

impl DayPage {
    pub fn ui(&mut self, ui: &mut egui::Ui) {
        FullPanel.show(ui, |ui| {
            
        });
    }
}
