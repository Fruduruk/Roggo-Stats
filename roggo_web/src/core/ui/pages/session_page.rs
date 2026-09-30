use eframe::egui::{self};
use uuid::Uuid;

use crate::core::{
    contract::{Playlist, session::SessionDto},
    ui::{
        components::{full_panel::FullPanel, split_ui::SplitUi, tab_control::Tab},
        theme::colors::colors,
        widgets::{match_cards, timeline},
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
                // SplitUi.show(ui, 0.618, |left_ui, right_ui| {
                ui.horizontal(|ui| {
                    if let Some(m) = session.matches.first() {
                        ui.heading(format!("{}", m.playlist));
                    }
                    ui.add_space(15.0);
                    // test_outline(right_ui, right_ui.available_rect_before_wrap(), colors(right_ui).error);
                    timeline::ui(ui, session, &self.hovered_match);
                });
                // });
                ui.add_space(5.0);
                SplitUi.show(ui, 1.618, |left_ui, right_ui| {
                    if let Some((response, match_guid)) =
                        match_cards::ui(left_ui, session, &self.selected_match)
                    {
                        if response.hovered() {
                            self.hovered_match = Some(match_guid);
                            self.selected_match = Some(match_guid);
                        }
                        if response.clicked() {
                            self.selected_match = Some(match_guid);
                        }
                    } else {
                        self.hovered_match = None;
                    }

                    if let Some(selected) = self.selected_match {
                        right_ui.label(format!("{}", selected));
                    }
                });
            });
        });
    }
}

fn test_outline(ui: &mut egui::Ui, rect: egui::Rect, color: egui::Color32) {
    ui.painter().rect_stroke(
        rect,
        1.0,
        egui::Stroke::new(1.0, color),
        egui::StrokeKind::Inside,
    );
}
