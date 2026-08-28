use crate::core::ui::theme::colors::colors;
use crate::core::ui::widgets::date_control::date_control;
use eframe::egui;
use jiff::civil::Date;

pub fn ui(ui: &mut egui::Ui, date: &mut Date, player_name: &Option<String>) {
    egui::Panel::top("header")
        .frame(
            egui::Frame::new()
                .fill(colors(ui).background)
                .inner_margin(5.0),
        )
        .show_separator_line(false)
        .show(ui, |ui| {
            ui.horizontal(|ui| {
                ui.heading("Roggo Stats");

                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    date_control(ui, date);

                    if let Some(name) = player_name {
                        ui.label(name);
                    }
                });
            });
        });
}
