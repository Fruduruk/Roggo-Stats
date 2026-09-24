use eframe::egui;
use itertools::Itertools;

use crate::core::{
    contract::session::{DetailedSessionDto, SessionDto}, links::to_tracker_network_link, time::{format_ms_min_seconds, format_ms_time, format_ms_time_without_seconds}, ui::{components::full_panel::FullPanel, mappers::map_arena, theme::colors::colors, widgets::timeline},
};
use egui_extras::{Column, TableBuilder};
#[derive(Default)]
pub struct SessionPage {}

impl SessionPage {
    pub fn ui(&mut self, ui: &mut egui::Ui, session: &SessionDto, player_name: &str) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical()
                .show(ui, |ui| {
                    timeline::ui(ui, session);
                });
        });
    }
}
