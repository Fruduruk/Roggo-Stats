use crate::core::ui::theme::colors::colors;
use eframe::egui;

pub fn ui(ui: &mut egui::Ui, player_name: &Option<String>) {
    egui::Panel::top("header")
        .frame(
            egui::Frame::new()
                .fill(colors(ui).background)
                .inner_margin(egui::Margin::symmetric(18, 3)),
        )
        .show_separator_line(false)
        .show(ui, |ui| {
            ui.horizontal(|ui| {
                ui.heading("Roggo Stats");

                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    if let Some(name) = player_name {
                        ui.label(name);
                    }
                    egui::widgets::global_theme_preference_switch(ui);
                });
            });
        });
}
