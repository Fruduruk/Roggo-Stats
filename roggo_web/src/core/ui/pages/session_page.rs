use std::fmt::Display;

use crate::core::ui::{
    components::{
        bee_swarm_comparison::{self, Filter, Statistic},
        four_cell_layout,
        full_panel::FullPanel,
        tab_control::Tab,
    },
    theme::colors::colors,
    widgets::{
        match_details, match_selector,
        multi_toggle::{MultiToggle, Orientation},
        timeline,
    },
};
use eframe::egui::{self};
use egui_extras::{Size, StripBuilder};
use roggo_contract::*;
use uuid::Uuid;

#[derive(Default, Copy, Clone, PartialEq, enum_iterator::Sequence)]
pub enum SessionViewMode {
    #[default]
    Analysis,
    Details,
}

impl Display for SessionViewMode {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Analysis => write!(f, "Analysis"),
            Self::Details => write!(f, "Details"),
        }
    }
}

#[derive(Default)]
pub struct SessionPage {
    selected_match: Option<Uuid>,
    hovered_match: Option<Uuid>,
    session_view_mode_toggle: MultiToggle<SessionViewMode>,
    statistic_toggle: MultiToggle<Statistic>,
    filter_toggle: MultiToggle<Filter>,
}

impl SessionPage {
    pub fn reset(&mut self) {
        self.selected_match = None;
        self.hovered_match = None;
    }

    pub fn ui(
        &mut self,
        ui: &mut egui::Ui,
        main_character: &Option<PlayerDto>,
        session: &SessionDto,
        session_details: &Option<SessionDetails>,
        new_tab: &mut Option<Tab>,
    ) {
        FullPanel.show(ui, |ui| {
            four_cell_layout::ui(
                ui,
                125.0,
                self,
                |this, ui| {
                    egui::Frame::new()
                        .inner_margin(5.0)
                        // .stroke(egui::Stroke::new(1.0, colors(ui).border))
                        .corner_radius(5)
                        .show(ui, |ui| {
                            this.top_left(ui, session);
                        });
                },
                |this, ui| {
                    egui::Frame::new()
                        .inner_margin(5.0)
                        // .stroke(egui::Stroke::new(1.0, colors(ui).border))
                        .corner_radius(5)
                        .show(ui, |ui| {
                            ui.take_available_space();
                            this.top_right(ui, session);
                        });
                },
                |this, ui| {
                    this.bottom_left(ui, session);
                },
                |this, ui| {
                    egui::Frame::new()
                        .inner_margin(5.0)
                        // .stroke(egui::Stroke::new(1.0, colors(ui).border))
                        .corner_radius(5)
                        .show(ui, |ui| {
                            ui.take_available_space();
                            this.bottom_right(
                                ui,
                                main_character,
                                session,
                                session_details,
                                new_tab,
                            );
                        });
                },
            );
        });
    }

    fn top_left(&mut self, ui: &mut egui::Ui, session: &SessionDto) {
        playlist_and_players(session, ui);
    }

    fn top_right(&mut self, ui: &mut egui::Ui, session: &SessionDto) {
        timeline::ui(ui, session, &self.selected_match.or(self.hovered_match));
    }

    fn bottom_left(&mut self, ui: &mut egui::Ui, session: &SessionDto) {
        ui.vertical_centered(|ui| {
            egui::Frame::new()
                .inner_margin(egui::Margin {
                    top: 10,
                    bottom: 5,
                    ..Default::default()
                })
                .show(ui, |ui| {
                    self.session_view_mode_toggle
                        .ui(ui, Orientation::Horizontal);
                });
        });
        egui::Frame::new()
            .inner_margin(5.0)
            .stroke(egui::Stroke::new(1.0, colors(ui).border))
            .corner_radius(5)
            .show(ui, |ui| {
                ui.take_available_space();
                egui::ScrollArea::vertical().show(ui, |ui| {
                    if let Some((response, match_guid)) =
                        match_selector::ui(ui, session, &self.selected_match)
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
                });
            });
    }

    fn bottom_right(
        &mut self,
        ui: &mut egui::Ui,
        main_character: &Option<PlayerDto>,
        session: &SessionDto,
        session_details: &Option<SessionDetails>,
        new_tab: &mut Option<Tab>,
    ) {
        match self.session_view_mode_toggle.get_state() {
            (SessionViewMode::Details, _) => self.show_match_details(ui, session, new_tab),
            (SessionViewMode::Analysis, _) => {
                self.show_analysis(ui, main_character, session_details)
            }
        }
    }

    fn show_match_details(
        &mut self,
        ui: &mut egui::Ui,
        session: &SessionDto,
        new_tab: &mut Option<Tab>,
    ) {
        let session_match_dto = self
            .selected_match
            .or(self.hovered_match)
            .and_then(|match_guid| session.matches.iter().find(|m| m.match_guid == match_guid));

        egui::Frame::new().inner_margin(5.0).show(ui, |ui| {
            match_details::ui(ui, session_match_dto, new_tab);
        });
    }

    fn show_analysis(
        &mut self,
        ui: &mut egui::Ui,
        main_character: &Option<PlayerDto>,
        session_details: &Option<SessionDetails>,
    ) {
        let (Some(session_details), Some(main_character)) = (session_details, main_character)
        else {
            return;
        };

        StripBuilder::new(ui)
            .size(Size::remainder())
            .size(Size::exact(150.0))
            .horizontal(|mut strip| {
                strip.cell(|left_ui| {
                    egui::Frame::new().inner_margin(5.0).show(left_ui, |ui| {
                        self.filter_toggle.ui(ui, Orientation::Horizontal);
                        ui.add_space(5.0);
                        bee_swarm_comparison::ui(
                            ui,
                            session_details,
                            main_character,
                            self.statistic_toggle.get_state(),
                            self.filter_toggle.get_state(),
                            self.selected_match.or(self.hovered_match),
                        );
                    });
                });

                strip.cell(|right_ui| {
                    egui::ScrollArea::vertical().show(right_ui, |ui: &mut egui::Ui| {
                        egui::Frame::new().inner_margin(5.0).show(ui, |ui| {
                            self.statistic_toggle.ui(ui, Orientation::Vertical);
                        });
                    });
                });
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
