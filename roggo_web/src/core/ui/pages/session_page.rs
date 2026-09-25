use eframe::egui;

use crate::core::{
    contract::session::{DetailedSessionDto, SessionDto}, links::to_tracker_network_link, time::{format_ms_min_seconds, format_ms_time, format_ms_time_without_seconds}, ui::{
        components::{full_panel::FullPanel, tab_control::Tab}, mappers::map_arena, theme::colors::colors, widgets::timeline,
    },
};
#[derive(Default)]
pub struct SessionPage {}

impl SessionPage {
    pub fn ui(
        &mut self,
        ui: &mut egui::Ui,
        session: &SessionDto,
        player_name: &str,
        selected_tab: &mut Tab,
    ) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical().show(ui, |ui| {
                timeline::ui(ui, session, selected_tab);
            });
        });
    }
}
