use crate::core::ui::{
    components::{
        bee_swarm_comparison, full_panel::FullPanel, split_ui::SplitUi, tab_control::Tab,
    },
    widgets::{beeswarm_plot, match_cards, match_details, timeline},
};
use eframe::egui::{self, vec2};
use roggo_contract::*;
use uuid::Uuid;
#[derive(Default)]
pub struct SessionPage {
    selected_match: Option<Uuid>,
    hovered_match: Option<Uuid>,
}

impl SessionPage {
    pub fn ui(
        &mut self,
        ui: &mut egui::Ui,
        main_character: &Option<PlayerDto>,
        session: &SessionDto,
        session_details: &Option<SessionDetails>,
        new_tab: &mut Option<Tab>,
    ) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical().show(ui, |ui| {
                self.header(ui, session);
                ui.add_space(10.0);
                self.match_selector(ui, session, new_tab);

                if let (Some(player), Some(session_details)) = (main_character, session_details) {
                    bee_swarm_comparison::ui(ui, session_details, player, |p| p.saves as f32);
                }
            });
        });
    }

    fn match_selector(
        &mut self,
        ui: &mut egui::Ui,
        session: &SessionDto,
        new_tab: &mut Option<Tab>,
    ) {
        SplitUi.show(ui, 1.618, 0.0, 0.0, |left_ui, right_ui| {
            if let Some((response, match_guid)) =
                match_cards::ui(left_ui, session, &self.selected_match)
            {
                if response.hovered() {
                    self.hovered_match = Some(match_guid);
                }
                if response.clicked() {
                    self.selected_match = if self.selected_match == Some(match_guid) {
                        None
                    } else {
                        Some(match_guid)
                    };
                }
            } else {
                self.hovered_match = None;
            }

            match_details::ui(
                right_ui,
                self.selected_match
                    .or(self.hovered_match)
                    .and_then(|selected| session.matches.iter().find(|m| m.match_guid == selected)),
                new_tab,
            );
        });
    }

    fn header(&mut self, ui: &mut egui::Ui, session: &SessionDto) {
        ui.horizontal(|ui| {
            playlist_and_players(session, ui);
            ui.add_space(15.0);
            timeline::ui(ui, session, &self.selected_match.or(self.hovered_match));
        });
    }
}

fn playlist_and_players(session: &SessionDto, ui: &mut egui::Ui) {
    ui.vertical(|ui| {
        ui.spacing_mut().item_spacing.y = 1.0;
        if let Some(m) = session.matches.first() {
            ui.label(
                egui::RichText::new(format!("{}", m.playlist))
                    .size(17.0)
                    .strong(),
            );

            ui.label(
                egui::RichText::new(
                    m.allies
                        .iter()
                        .map(|p| p.display_name.as_str())
                        .collect::<Vec<_>>()
                        .join(", "),
                )
                .font(egui::FontId::new(
                    12.0,
                    egui::FontFamily::Name("player_name".into()),
                ))
                .color(ui.visuals().weak_text_color()),
            );
        }
    });
}

// fn test_outline(ui: &mut egui::Ui, rect: egui::Rect, color: egui::Color32) {
//     ui.painter().rect_stroke(
//         rect,
//         1.0,
//         egui::Stroke::new(1.0, color),
//         egui::StrokeKind::Inside,
//     );
// }
