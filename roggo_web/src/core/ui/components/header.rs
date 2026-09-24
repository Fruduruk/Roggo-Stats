use std::ops::Index;

use crate::core::{
    api_result::APIResult,
    contract::day::DaysPlayedDto,
    tasks,
    ui::{
        components::tab_control::Tab,
        theme::colors::colors,
        widgets::date_control::{self},
    },
};
use eframe::egui::{self};
use futures_channel::mpsc::Sender;
use jiff::civil::Date;

pub fn ui(
    ui: &mut egui::Ui,
    player_name: &Option<String>,
    days_played: &Option<Vec<Date>>,
    date: &mut Date,
    sender: &Sender<APIResult>,
    current_tab: &mut Tab,
) {
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
                    if let Some(days_played) = days_played.as_ref()
                        && let Some(last) = days_played.last()
                    {
                        let current_date = *date;
                        let days_played_index = days_played
                            .iter()
                            .position(|d| d == date)
                            .unwrap_or(days_played.len() - 1);

                        if ui.button("←").clicked() {
                            let new_date = days_played.get(days_played_index.saturating_sub(1)).unwrap_or(&current_date);
                            *date = *new_date;
                        }

                        date_control::ui(ui, date);

                        if !days_played.contains(date) {
                            *date = *last;
                        }

                        if ui.button("→").clicked() {
                            let new_date = days_played.get(days_played_index + 1).unwrap_or(&current_date);
                            *date = *new_date;
                        }

                        if current_date != *date {
                            tasks::load_day(sender.clone(), *date);
                            *current_tab = Tab::Day;
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
