use crate::core::{
    api_result::APIResult,
    tasks,
    ui::{
        components::{
            full_panel::FullPanel,
            split_ui::{self, SplitUi},
            tab_control::Tab,
        },
        widgets::{date_picker, session_card},
    },
};
use eframe::egui;
use egui_extras::{Size, StripBuilder};
use futures_channel::mpsc::Sender;
use jiff::civil::Date;
use roggo_contract::*;
use uuid::Uuid;

#[derive(Default)]
pub struct DayPage {}

impl DayPage {
    pub fn ui(
        &mut self,
        ui: &mut egui::Ui,
        day_dto: &DayDto,
        days_played: &Option<Vec<Date>>,
        date: &mut Date,
        sender: &Sender<APIResult>,
        session_match_list: &mut Vec<Uuid>,
        new_tab: &mut Option<Tab>,
    ) {
        FullPanel.show(ui, |ui| {
            let left_width = 260.0;
            let min_right_width = 250.0;

            if ui.available_width() < left_width + min_right_width {
                return;
            }

            StripBuilder::new(ui)
                .size(Size::exact(left_width))
                .size(Size::remainder())
                .horizontal(|mut strip| {
                    strip.cell(|left_ui| {
                        top_center_scope(left_ui, egui::vec2(220.0, 30.0), |ui| {
                            date_picker::ui(ui, days_played, sender, new_tab, date);
                        });

                        self.show_day_stats(left_ui, day_dto);
                    });

                    strip.cell(|right_ui| {
                        egui::ScrollArea::vertical().show(right_ui, |ui: &mut egui::Ui| {
                            self.show_session_cards(
                                ui,
                                day_dto,
                                sender,
                                session_match_list,
                                new_tab,
                            );
                        });
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
        new_tab: &mut Option<Tab>,
    ) {
        for session in &day_dto.sessions {
            if session_card::ui(ui, session).clicked() {
                session_match_list.clear();
                session_match_list.extend(session.matches.iter().map(|s| s.match_guid));
                tasks::load_session(sender.clone(), session_match_list.clone());
                tasks::load_session_details(sender.clone(), session_match_list.clone());
                *new_tab = Some(Tab::Session);
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

        egui::Grid::new("day_stats")
            .num_columns(2)
            .spacing([24.0, 8.0])
            .striped(true)
            .show(ui, |ui| {
                ui.label("Matches");
                ui.strong(match_count.to_string());
                ui.end_row();

                ui.label("Record");
                ui.strong(format!("{won} won - {lost} lost"));
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
fn top_center_scope<R>(
    ui: &mut egui::Ui,
    size: egui::Vec2,
    add_contents: impl FnOnce(&mut egui::Ui) -> R,
) -> egui::InnerResponse<R> {
    let available = ui.available_rect_before_wrap();

    let rect = egui::Rect::from_min_size(
        egui::pos2(available.center().x - size.x / 2.0, available.top()),
        size,
    );

    ui.scope_builder(egui::UiBuilder::new().max_rect(rect), add_contents)
}
