use eframe::egui;
use jiff::civil::Date;

use crate::core::ui::{
    components::full_panel::FullPanel, widgets::date_control::date_control,
};

#[derive(Default)]
pub struct DayPage {}

impl DayPage {
    pub fn ui(&mut self, ui: &mut egui::Ui, date: &mut Date) {
        FullPanel.show(ui, |ui| {
        });
    }
}
