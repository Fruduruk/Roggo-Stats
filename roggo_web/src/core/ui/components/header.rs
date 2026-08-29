use crate::core::ui::{theme::colors::colors, widgets::date_control::{self}};
use eframe::egui::{self};
use jiff::civil::Date;

pub fn ui(ui: &mut egui::Ui, player_name: &Option<String>, date: &mut Date) {
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

            mid_rect_scope(ui, egui::vec2(220.0, ui.max_rect().height()), |ui| {
                ui.horizontal(|ui| {
                    if ui.button("←").clicked() {
                        if let Ok(yesterday) = date.yesterday() {
                            *date = yesterday;
                        }
                    }

                    date_control::ui(ui, date);

                    if ui.button("→").clicked() {
                        if let Ok(tomorrow) = date.tomorrow() {
                            *date = tomorrow;
                        }
                    }
                });
            });
        });
}

fn mid_rect_scope<R>(
    ui: &mut egui::Ui,
    rect: egui::Vec2,
    add_contents: impl FnOnce(&mut egui::Ui) -> R,
) -> egui::InnerResponse<R> {
    let center_rect = egui::Rect::from_center_size(ui.max_rect().center(), rect);

    ui.scope_builder(
        egui::UiBuilder::new()
            .max_rect(center_rect)
            .layout(egui::Layout::centered_and_justified(
                egui::Direction::LeftToRight,
            )),
        |ui| add_contents(ui),
    )
}
