use eframe::egui;

use crate::core::{ contract::session::DetailedSessionDto, ui::components::full_panel::FullPanel};

#[derive(Default)]
pub struct SessionPage {}

impl SessionPage {
    pub fn ui(
        &mut self,
        ui: &mut egui::Ui,
        detailed_session: &DetailedSessionDto,
        player_name: &str,
    ) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical().show(ui, |ui| {
                egui::Grid::new("match grid")
                    .num_columns(6)
                    .spacing([24.0, 8.0])
                    .striped(true)
                    .show(ui, |ui| {
                        ui.strong("#");
                        ui.strong("Time");
                        ui.strong("Duration");
                        ui.strong("Arena");
                        ui.strong("Score");
                        ui.strong("Opponents");
                        ui.end_row();
                        let mut i = 0;
                        for session_match in &detailed_session.session_matches {
                            i += 1;
                            ui.label(i.to_string());
                            ui.label(session_match.created_at.to_string());
                            ui.label((session_match.ended_at - session_match.created_at).to_string());
                            ui.label(session_match.won.unwrap_or_default().to_string());
                            ui.end_row();
                        }
                    });
            });
        });
    }
}
