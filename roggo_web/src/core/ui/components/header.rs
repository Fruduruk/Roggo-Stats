use crate::core::{
    api_result::APIResult,
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
use roggo_contract::PlayerDto;

pub fn ui(
    ui: &mut egui::Ui,
    player_name: &Option<PlayerDto>,
    sender: &Sender<APIResult>,
    new_tab: &mut Option<Tab>,
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
                    if let Some(dto) = player_name {
                        ui.label(&dto.display_name);
                    }

                    egui::widgets::global_theme_preference_switch(ui);
                });
            });
        });
}

