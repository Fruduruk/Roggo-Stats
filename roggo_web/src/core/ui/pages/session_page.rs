use eframe::egui;
use uuid::Uuid;

use crate::core::{
    contract::session::{DetailedSessionDto, SessionDto}, links::to_tracker_network_link, time::{format_ms_min_seconds, format_ms_time, format_ms_time_without_seconds}, ui::{
        components::{full_panel::FullPanel, tab_control::Tab}, mappers::map_arena, theme::colors::colors, widgets::{match_cards, timeline},
    },
};
#[derive(Default)]
pub struct SessionPage {
    selected_match: Option<Uuid>,
    hovered_match: Option<Uuid>,
}

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
                timeline::ui(ui, session, &self.hovered_match);

                ui.columns(2, |columns| {
                    if let Some((response,match_guid)) = match_cards::ui(&mut columns[0], session) {
                        if response.hovered() {
                            self.hovered_match = Some(match_guid);
                        }
                        if response.clicked() {
                            self.selected_match = Some(match_guid);
                        }
                    }
                    if let Some(selected) = self.selected_match {
                        columns[1].label(format!("{}", selected));
                    }
                });
            });
        });
    }
}

fn test_outline(ui: &mut egui::Ui, rect: egui::Rect) {
    ui.painter().rect_stroke(
        rect,
        1.0,
        egui::Stroke::new(1.0, colors(ui).on_panel),
        egui::StrokeKind::Inside,
    );
}
