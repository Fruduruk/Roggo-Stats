use crate::core::ui::{components::tab_control::TabControl, theme::colors::colors};
use eframe::egui::{self, RichText};

pub fn ui(ui: &mut egui::Ui, tab_control: &mut TabControl) {
    egui::Panel::top("header")
        .frame(
            egui::Frame::new()
                .fill(colors(ui).background)
                .inner_margin(egui::Margin::symmetric(8, 4)),
        )
        .show_separator_line(false)
        .show(ui, |ui| {
            ui.horizontal_centered(|ui| {
                ui.label(
                    RichText::new("Roggo Stats")
                        .size(22.0)
                        .family(egui::FontFamily::Name("title".into())),
                );
                tab_control.ui(ui);
            });
        });
}
