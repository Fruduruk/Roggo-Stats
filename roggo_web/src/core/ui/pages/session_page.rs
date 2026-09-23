use eframe::egui;
use itertools::Itertools;

use crate::core::{
    contract::session::{DetailedSessionDto, SessionDto},
    links::to_tracker_network_link,
    time::{format_ms_min_seconds, format_ms_time, format_ms_time_without_seconds},
    ui::{components::full_panel::FullPanel, mappers::map_arena, theme::colors::colors},
};
use egui_extras::{Column, TableBuilder};
#[derive(Default)]
pub struct SessionPage {}

impl SessionPage {
    pub fn ui(&mut self, ui: &mut egui::Ui, session: &SessionDto, player_name: &str) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical().scroll_bar_visibility(egui::scroll_area::ScrollBarVisibility::AlwaysHidden).show(ui, |ui| {
                TableBuilder::new(ui)
                    .striped(true)
                    .cell_layout(egui::Layout::left_to_right(egui::Align::Center))
                    .column(Column::auto()) // Result
                    .column(Column::auto()) // Time
                    .column(Column::auto()) // Duration
                    .column(Column::auto()) // Score
                    .column(Column::auto()) // MVP
                    .column(Column::auto()) // Tracker
                    .header(28.0, |mut header| {
                        header.col(|ui| {
                            ui.strong("Result");
                        });

                        header.col(|ui| {
                            ui.strong("Time");
                        });

                        header.col(|ui| {
                            ui.strong("Duration");
                        });

                        header.col(|ui| {
                            ui.strong("Score");
                        });

                        header.col(|ui| {
                            ui.strong("MVP");
                        });

                        header.col(|ui| {
                            ui.strong("Tracker");
                        });
                    })
                    .body(|mut body| {
                        for session_match in &session.matches {
                            body.row(32.0, |mut row| {
                                row.col(|ui| {
                                    ui.label(
                                        egui::RichText::new(match session_match.won {
                                            Some(true) => "Win",
                                            Some(false) => "Loss",
                                            None => "Unknown",
                                        })
                                        .color(
                                            match session_match.won {
                                                Some(true) => colors(ui).success,
                                                Some(false) => colors(ui).error,
                                                None => colors(ui).on_panel,
                                            },
                                        ),
                                    );
                                });

                                row.col(|ui| {
                                    ui.label(format_ms_time_without_seconds(
                                        session_match.created_at,
                                    ));
                                });

                                row.col(|ui| {
                                    ui.label(format_ms_min_seconds(session_match.duration));
                                });

                                row.col(|ui| {
                                    ui.label(format!(
                                        "{} : {}",
                                        session_match.own_score, session_match.enemy_score,
                                    ));
                                });

                                row.col(|ui| {
                                    ui.label(match session_match.mvp_type {
                                        crate::core::contract::MVPType::MVP => "⭐",
                                        crate::core::contract::MVPType::ACE => "👍",
                                        crate::core::contract::MVPType::Nothing => "",
                                    });
                                });

                                row.col(|ui| {
                                    if ui.button("open").clicked() {
                                        for enemy in &session_match.enemies {
                                            if let Some(url) = to_tracker_network_link(
                                                &enemy.primary_id,
                                                &enemy.display_name,
                                            ) {
                                                ui.ctx().open_url(egui::OpenUrl::new_tab(url));
                                            }
                                        }
                                    }
                                });
                            });
                        }
                    });
            });
        });
    }
}
