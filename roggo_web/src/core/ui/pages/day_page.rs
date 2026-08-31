use eframe::egui;
use futures_channel::mpsc::Sender;
use uuid::Uuid;

use crate::core::{
    api_result::APIResult,
    contract::DayDto,
    tasks,
    ui::{
        components::{full_panel::FullPanel, tab_control::Tab},
        widgets::session_card,
    },
};

#[derive(Default)]
pub struct DayPage {}

impl DayPage {
    pub fn ui(
        &mut self,
        ui: &mut egui::Ui,
        day_dto: &DayDto,
        sender: &Sender<APIResult>,
        session_match_list: &mut Vec<Uuid>,
        selected_tab: &mut Tab,
    ) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical().show(ui, |ui| {
                ui.columns(2, |columns| {
                    let left_column_ui = &mut columns[0];
                    self.show_session_cards(
                        left_column_ui,
                        day_dto,
                        sender,
                        session_match_list,
                        selected_tab,
                    );
                    let right_column_ui = &mut columns[1];
                    self.show_day_stats(right_column_ui, day_dto);
                });
            });
        });
    }

    fn show_session_cards(
        &self,
        ui: &mut egui::Ui,
        day_dto: &DayDto,
        sender: &Sender<APIResult>,
        session_match_list: &mut Vec<Uuid>,
        selected_tab: &mut Tab,
    ) {
        for session in &day_dto.sessions {
            if session_card::ui(ui, session).clicked() {
                session_match_list.clear();
                session_match_list.extend(session.matches.iter().map(|s| s.match_guid));
                tasks::load_detailed_session(sender.clone(), session_match_list.clone());
                *selected_tab = Tab::Session;
            }
        }
    }

    fn show_day_stats(&self, ui: &mut egui::Ui, day_dto: &DayDto) {
        let matches = day_dto
            .sessions
            .iter()
            .flat_map(|session| &session.matches)
            .collect::<Vec<_>>();

        let match_count = matches.len();
        let won = matches.iter().filter(|m| m.won).count();
        let lost = match_count - won;

        let winrate = if match_count > 0 {
            won as f32 / match_count as f32 * 100.0
        } else {
            0.0
        };

        let total_duration_ms: i64 = matches.iter().map(|m| m.ended_at - m.created_at).sum();

        let total_minutes = total_duration_ms / 60_000;

        let longest_session_minutes = day_dto
            .sessions
            .iter()
            .map(|session| (session.ended_at - session.created_at) / 60_000)
            .max()
            .unwrap_or(0);

        egui::Frame::new()
            .inner_margin(egui::Margin::symmetric(16, 0))
            .show(ui, |ui| {
                egui::Grid::new("day_stats")
                    .num_columns(2)
                    .spacing([24.0, 8.0])
                    .striped(true)
                    .show(ui, |ui| {
                        ui.label("Matches");
                        ui.strong(match_count.to_string());
                        ui.end_row();

                        ui.label("Record");
                        ui.strong(format!("{won} - {lost}"));
                        ui.end_row();

                        ui.label("Winrate");
                        ui.strong(format!("{winrate:.0}%"));
                        ui.end_row();

                        ui.label("Playtime");
                        ui.strong(format_duration(total_minutes));
                        ui.end_row();

                        ui.label("Sessions");
                        ui.strong(day_dto.sessions.len().to_string());
                        ui.end_row();

                        ui.label("Longest session");
                        ui.strong(format_duration(longest_session_minutes));
                        ui.end_row();
                    });
            });
    }
}
fn format_duration(minutes: i64) -> String {
    let hours = minutes / 60;
    let minutes = minutes % 60;

    if hours > 0 {
        format!("{hours}h {minutes} min")
    } else {
        format!("{minutes} min")
    }
}
