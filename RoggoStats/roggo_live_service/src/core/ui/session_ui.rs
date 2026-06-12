use std::sync::{Arc, Mutex};

use crate::core::{
    contract::{
        DetailedAverageAdvancedStatsDto, DetailedAverageCoreStatsDto, DetailedSessionDto, MVPType,
    },
    ui::{
        match_overview_ui::{GREEN, GREY, RED},
        tasks,
    },
};
use chrono::{DateTime, Local, Utc};
use eframe::egui::{self, Align2, Color32, Context, FontId, RichText, Sense, Stroke, Vec2};
use uuid::Uuid;

#[derive(Default)]
pub struct Content {
    pub detailed_session: Option<DetailedSessionDto>,
}

#[derive(Default)]
pub struct SessionUi {
    content: Arc<Mutex<Content>>,
}

impl SessionUi {
    pub fn reload(&self, context: Context, match_guids: Vec<Uuid>) {
        tasks::load_detailed_session(context, self.content.clone(), match_guids.clone());
    }

    pub fn ui(&self, ui: &mut egui::Ui, player_name: &String) {
        let Ok(content) = self.content.lock() else {
            ui.label("Failed to lock match content.");
            return;
        };

        let Some(session) = &content.detailed_session else {
            ui.centered_and_justified(|ui| {
                ui.label("No session selected.");
            });
            return;
        };

        egui::ScrollArea::vertical()
            .auto_shrink([false, false])
            .show(ui, |ui| {
                ui.heading("Session Stats");
                ui.add_space(8.0);

                self.render_match_timeline(ui, session);
                ui.add_space(18.0);
                ui.separator();
                ui.add_space(12.0);

                self.render_core_stats(ui, session, player_name);
                ui.add_space(18.0);
                ui.separator();
                ui.add_space(12.0);

                self.render_advanced_stats(ui, session, player_name);
            });
    }

    fn render_match_timeline(&self, ui: &mut egui::Ui, session: &DetailedSessionDto) {
        ui.heading("Timeline");

        let visible_matches: Vec<_> = session
            .session_matches
            .iter()
            .filter(|m| !m.hidden)
            .collect();

        if visible_matches.is_empty() {
            ui.label(RichText::new("No visible matches available.").weak());
            return;
        }

        let Some(first_started_at) = visible_matches.iter().map(|m| m.created_at).min() else {
            return;
        };

        let Some(last_ended_at) = visible_matches.iter().map(|m| m.ended_at).max() else {
            return;
        };

        let total_duration = (last_ended_at - first_started_at).max(1) as f32;

        let desired_size = Vec2::new(ui.available_width(), 150.0);
        let (rect, _) = ui.allocate_exact_size(desired_size, Sense::hover());
        let painter = ui.painter_at(rect);

        let left_padding = 64.0;
        let right_padding = 16.0;
        let top_padding = 18.0;

        let chart_left = rect.left() + left_padding;
        let chart_right = rect.right() - right_padding;
        let chart_top = rect.top() + top_padding;

        let chart_width = chart_right - chart_left;

        let bar_y = chart_top + 34.0;
        let duration_y = chart_top + 72.0;
        let x_axis_y = chart_top + 104.0;

        let bar_height = 18.0;

        painter.line_segment(
            [
                egui::pos2(chart_left, chart_top),
                egui::pos2(chart_left, x_axis_y),
            ],
            Stroke::new(1.0, Color32::GRAY),
        );

        painter.line_segment(
            [
                egui::pos2(chart_left, x_axis_y),
                egui::pos2(chart_right, x_axis_y),
            ],
            Stroke::new(1.0, Color32::GRAY),
        );

        painter.text(
            egui::pos2(chart_left - 8.0, bar_y),
            Align2::RIGHT_CENTER,
            "Match",
            FontId::proportional(11.0),
            Color32::GRAY,
        );

        painter.text(
            egui::pos2(chart_left - 8.0, duration_y),
            Align2::RIGHT_CENTER,
            "Duration",
            FontId::proportional(11.0),
            Color32::GRAY,
        );

        let tick_count = nice_tick_count(chart_width);

        for tick in 0..=tick_count {
            let t = tick as f32 / tick_count as f32;
            let x = chart_left + chart_width * t;
            let timestamp =
                first_started_at + ((last_ended_at - first_started_at) as f32 * t) as i64;

            painter.line_segment(
                [egui::pos2(x, x_axis_y), egui::pos2(x, x_axis_y + 5.0)],
                Stroke::new(1.0, Color32::GRAY),
            );

            if tick > 0 && tick < tick_count {
                painter.line_segment(
                    [egui::pos2(x, chart_top), egui::pos2(x, x_axis_y)],
                    Stroke::new(0.5, Color32::from_gray(55)),
                );
            }

            let align = if tick == 0 {
                Align2::LEFT_TOP
            } else if tick == tick_count {
                Align2::RIGHT_TOP
            } else {
                Align2::CENTER_TOP
            };

            painter.text(
                egui::pos2(x, x_axis_y + 8.0),
                align,
                format_clock_time(timestamp),
                FontId::monospace(11.0),
                Color32::GRAY,
            );
        }

        for session_match in visible_matches {
            let start_offset = (session_match.created_at - first_started_at).max(0) as f32;
            let end_offset = (session_match.ended_at - first_started_at).max(0) as f32;

            let x1 = chart_left + chart_width * (start_offset / total_duration);
            let x2 = chart_left + chart_width * (end_offset / total_duration);

            let min_width = 3.0;
            let bar_right = x2.max(x1 + min_width);

            let color = match session_match.won {
                Some(true) => GREEN,
                Some(false) => RED,
                None => GREY,
            };

            let bar_rect = egui::Rect::from_min_max(
                egui::pos2(x1, bar_y - bar_height / 2.0),
                egui::pos2(bar_right, bar_y + bar_height / 2.0),
            );

            painter.rect_filled(bar_rect, 3.0, color);

            let marker = match session_match.mvp_type {
                MVPType::MVP => Some("MVP"),
                MVPType::ACE => Some("ACE"),
                MVPType::Nothing => None,
            };

            if let Some(marker) = marker {
                painter.text(
                    bar_rect.center(),
                    Align2::CENTER_CENTER,
                    marker,
                    FontId::monospace(10.0),
                    Color32::WHITE,
                );
            }

            let duration = (session_match.ended_at - session_match.created_at).max(0);
            let duration_label = format_duration(duration);

            let duration_center_x = (x1 + bar_right) / 2.0;

            painter.text(
                egui::pos2(duration_center_x, duration_y),
                Align2::CENTER_CENTER,
                duration_label,
                FontId::monospace(10.0),
                Color32::GRAY,
            );
        }

        ui.horizontal(|ui| {
            timeline_legend(ui, GREEN, "Won");
            timeline_legend(ui, RED, "Lost");
            timeline_legend(ui, GREY, "Unknown");
        });
    }
    fn render_core_stats(
        &self,
        ui: &mut egui::Ui,
        session: &DetailedSessionDto,
        player_name: &str,
    ) {
        ui.heading("Core Stats");

        let mut bars: Vec<CoreBarValue> = Vec::new();

        if let Some(player) = session
            .own_team_player_averages
            .iter()
            .find(|p| p.username == player_name)
        {
            bars.push(CoreBarValue {
                label: "You".to_string(),
                stats: &player.average_core_stats,
                color: Color32::from_rgb(90, 145, 230),
            });
        }

        let mates: Vec<_> = session
            .own_team_player_averages
            .iter()
            .filter(|p| p.username != player_name)
            .collect();

        for mate in &mates {
            bars.push(CoreBarValue {
                label: mate.username.clone(),
                stats: &mate.average_core_stats,
                color: Color32::from_rgb(180, 130, 220),
            });
        }

        if mates.is_empty() {
            if let Some(team_average) = &session.average_team_player_core_stats {
                bars.push(CoreBarValue {
                    label: "Mate Ø".to_string(),
                    stats: team_average,
                    color: Color32::from_rgb(240, 170, 70),
                });
            }
        }

        if let Some(enemy_average) = &session.average_enemy_core_stats {
            bars.push(CoreBarValue {
                label: "Enemy Ø".to_string(),
                stats: enemy_average,
                color: Color32::from_rgb(210, 80, 80),
            });
        }

        if bars.is_empty() {
            ui.label(RichText::new("No core stats available.").weak());
            return;
        }

        self.core_chart(ui, "Score", &bars, |s| s.average_score, 1);
        self.core_chart(ui, "Goals", &bars, |s| s.average_goals, 2);
        self.core_chart(ui, "Shots", &bars, |s| s.average_shots, 2);
        self.core_chart(ui, "Assists", &bars, |s| s.average_assists, 2);
        self.core_chart(ui, "Saves", &bars, |s| s.average_saves, 2);
        self.core_chart(ui, "Demos", &bars, |s| s.average_demos, 2);
    }

    fn core_chart(
        &self,
        ui: &mut egui::Ui,
        title: &str,
        bars: &[CoreBarValue<'_>],
        value_selector: fn(&DetailedAverageCoreStatsDto) -> f64,
        decimals: usize,
    ) {
        ui.add_space(10.0);
        ui.label(RichText::new(title).strong());

        let raw_max_value = bars
            .iter()
            .map(|bar| value_selector(bar.stats))
            .fold(0.0_f64, f64::max);

        let max_value = if raw_max_value <= 0.0 {
            0.01
        } else {
            raw_max_value * 1.15
        };

        let desired_size = Vec2::new(ui.available_width(), 180.0);
        let (rect, _) = ui.allocate_exact_size(desired_size, Sense::hover());
        let painter = ui.painter_at(rect);

        let left_padding = 54.0;
        let bottom_padding = 34.0;
        let top_padding = 10.0;
        let right_padding = 12.0;

        let chart_left = rect.left() + left_padding;
        let chart_right = rect.right() - right_padding;
        let chart_top = rect.top() + top_padding;
        let chart_bottom = rect.bottom() - bottom_padding;

        let chart_width = chart_right - chart_left;
        let chart_height = chart_bottom - chart_top;

        painter.line_segment(
            [
                egui::pos2(chart_left, chart_top),
                egui::pos2(chart_left, chart_bottom),
            ],
            Stroke::new(1.0, Color32::GRAY),
        );

        painter.line_segment(
            [
                egui::pos2(chart_left, chart_bottom),
                egui::pos2(chart_right, chart_bottom),
            ],
            Stroke::new(1.0, Color32::GRAY),
        );

        let tick_count = 4;

        for tick in 0..=tick_count {
            let t = tick as f32 / tick_count as f32;
            let y = chart_bottom - chart_height * t;
            let value = max_value * t as f64;

            painter.line_segment(
                [egui::pos2(chart_left - 4.0, y), egui::pos2(chart_left, y)],
                Stroke::new(1.0, Color32::GRAY),
            );

            painter.text(
                egui::pos2(chart_left - 6.0, y),
                Align2::RIGHT_CENTER,
                format!("{:.*}", decimals, value),
                FontId::monospace(11.0),
                Color32::GRAY,
            );

            if tick > 0 {
                painter.line_segment(
                    [egui::pos2(chart_left, y), egui::pos2(chart_right, y)],
                    Stroke::new(0.5, Color32::from_gray(55)),
                );
            }
        }

        let bar_count = bars.len().max(1) as f32;
        let slot_width = chart_width / bar_count;
        let bar_width = slot_width * 0.55;

        for (index, bar) in bars.iter().enumerate() {
            let value = value_selector(bar.stats);
            let center_x = chart_left + slot_width * (index as f32 + 0.5);
            let normalized = (value / max_value).clamp(0.0, 1.0) as f32;
            let bar_height = chart_height * normalized;

            let bar_rect = egui::Rect::from_min_max(
                egui::pos2(center_x - bar_width / 2.0, chart_bottom - bar_height),
                egui::pos2(center_x + bar_width / 2.0, chart_bottom),
            );

            painter.rect_filled(bar_rect, 3.0, bar.color);

            painter.text(
                egui::pos2(center_x, bar_rect.top() - 4.0),
                Align2::CENTER_BOTTOM,
                format!("{:.*}", decimals, value),
                FontId::monospace(11.0),
                Color32::WHITE,
            );

            painter.text(
                egui::pos2(center_x, chart_bottom + 6.0),
                Align2::CENTER_TOP,
                &bar.label,
                FontId::proportional(11.0),
                Color32::GRAY,
            );
        }
    }

    fn render_advanced_stats(
        &self,
        ui: &mut egui::Ui,
        session: &DetailedSessionDto,
        player_name: &str,
    ) {
        ui.heading("Advanced Averages");

        let mut players: Vec<AdvancedBarValue> = Vec::new();

        for player in &session.own_team_player_averages {
            let Some(stats) = &player.average_advanced_stats else {
                continue;
            };

            let label = if player.username == player_name {
                "You".to_string()
            } else {
                player.username.clone()
            };

            players.push(AdvancedBarValue {
                label,
                stats,
                color: if player.username == player_name {
                    Color32::from_rgb(90, 145, 230)
                } else {
                    Color32::from_rgb(180, 130, 220)
                },
            });
        }

        let team_average = session
            .average_team_player_advanced_stats
            .as_ref()
            .map(|stats| AdvancedBarValue {
                label: "Mate Ø".to_string(),
                stats,
                color: Color32::from_rgb(240, 170, 70),
            });

        if players.is_empty() && team_average.is_none() {
            ui.label(RichText::new("No advanced stats available.").weak());
            return;
        }

        self.advanced_chart(ui, "Boosting %", &players, team_average.as_ref(), |s| {
            s.average_percent_boosting
        });

        self.advanced_chart(ui, "Demolished %", &players, team_average.as_ref(), |s| {
            s.average_percent_demolished
        });

        self.advanced_chart(ui, "On ground %", &players, team_average.as_ref(), |s| {
            s.average_percent_on_ground
        });

        self.advanced_chart(ui, "On wall %", &players, team_average.as_ref(), |s| {
            s.average_percent_on_wall
        });

        self.advanced_chart(ui, "Powersliding %", &players, team_average.as_ref(), |s| {
            s.average_percent_powersliding
        });

        self.advanced_chart(ui, "Supersonic %", &players, team_average.as_ref(), |s| {
            s.average_percent_supersonic
        });
    }

    fn advanced_chart(
        &self,
        ui: &mut egui::Ui,
        title: &str,
        players: &[AdvancedBarValue<'_>],
        team_average: Option<&AdvancedBarValue<'_>>,
        value_selector: fn(&DetailedAverageAdvancedStatsDto) -> f64,
    ) {
        ui.add_space(10.0);
        ui.label(RichText::new(title).strong());

        let mut bars: Vec<(&str, f64, Color32)> = players
            .iter()
            .map(|p| (p.label.as_str(), value_selector(p.stats), p.color))
            .collect();

        if let Some(team_average) = team_average {
            bars.push((
                team_average.label.as_str(),
                value_selector(team_average.stats),
                team_average.color,
            ));
        }

        if bars.is_empty() {
            ui.label(RichText::new("No data available.").weak());
            return;
        }

        let raw_max_value = bars
            .iter()
            .map(|(_, value, _)| *value)
            .fold(0.0_f64, f64::max);

        let max_value = if raw_max_value <= 0.0 {
            0.01
        } else {
            raw_max_value * 1.15
        };

        let desired_size = Vec2::new(ui.available_width(), 180.0);
        let (rect, _) = ui.allocate_exact_size(desired_size, Sense::hover());
        let painter = ui.painter_at(rect);

        let left_padding = 54.0;
        let bottom_padding = 34.0;
        let top_padding = 10.0;
        let right_padding = 12.0;

        let chart_left = rect.left() + left_padding;
        let chart_right = rect.right() - right_padding;
        let chart_top = rect.top() + top_padding;
        let chart_bottom = rect.bottom() - bottom_padding;

        let chart_width = chart_right - chart_left;
        let chart_height = chart_bottom - chart_top;

        painter.line_segment(
            [
                egui::pos2(chart_left, chart_top),
                egui::pos2(chart_left, chart_bottom),
            ],
            Stroke::new(1.0, Color32::GRAY),
        );

        painter.line_segment(
            [
                egui::pos2(chart_left, chart_bottom),
                egui::pos2(chart_right, chart_bottom),
            ],
            Stroke::new(1.0, Color32::GRAY),
        );

        let tick_count = 4;

        for tick in 0..=tick_count {
            let t = tick as f32 / tick_count as f32;
            let y = chart_bottom - chart_height * t;
            let value = max_value * t as f64;

            painter.line_segment(
                [egui::pos2(chart_left - 4.0, y), egui::pos2(chart_left, y)],
                Stroke::new(1.0, Color32::GRAY),
            );

            painter.text(
                egui::pos2(chart_left - 6.0, y),
                Align2::RIGHT_CENTER,
                format!("{:.1}%", value * 100.0),
                FontId::monospace(11.0),
                Color32::GRAY,
            );

            if tick > 0 {
                painter.line_segment(
                    [egui::pos2(chart_left, y), egui::pos2(chart_right, y)],
                    Stroke::new(0.5, Color32::from_gray(55)),
                );
            }
        }

        let bar_count = bars.len().max(1) as f32;
        let slot_width = chart_width / bar_count;
        let bar_width = slot_width * 0.55;

        for (index, (label, value, color)) in bars.iter().enumerate() {
            let center_x = chart_left + slot_width * (index as f32 + 0.5);
            let normalized = (*value / max_value).clamp(0.0, 1.0) as f32;
            let bar_height = chart_height * normalized;

            let bar_rect = egui::Rect::from_min_max(
                egui::pos2(center_x - bar_width / 2.0, chart_bottom - bar_height),
                egui::pos2(center_x + bar_width / 2.0, chart_bottom),
            );

            painter.rect_filled(bar_rect, 3.0, *color);

            painter.text(
                egui::pos2(center_x, bar_rect.top() - 4.0),
                Align2::CENTER_BOTTOM,
                format!("{:.1}%", value * 100.0),
                FontId::monospace(11.0),
                Color32::WHITE,
            );

            painter.text(
                egui::pos2(center_x, chart_bottom + 6.0),
                Align2::CENTER_TOP,
                *label,
                FontId::proportional(11.0),
                Color32::GRAY,
            );
        }
    }
}

struct AdvancedBarValue<'a> {
    label: String,
    stats: &'a DetailedAverageAdvancedStatsDto,
    color: Color32,
}
struct CoreBarValue<'a> {
    label: String,
    stats: &'a DetailedAverageCoreStatsDto,
    color: Color32,
}

fn timeline_legend(ui: &mut egui::Ui, color: Color32, label: &str) {
    let (rect, _) = ui.allocate_exact_size(Vec2::new(10.0, 10.0), Sense::hover());
    ui.painter().rect_filled(rect, 2.0, color);
    ui.label(RichText::new(label).weak());
    ui.add_space(8.0);
}

fn nice_tick_count(width: f32) -> i32 {
    if width < 360.0 {
        3
    } else if width < 600.0 {
        4
    } else if width < 900.0 {
        6
    } else {
        8
    }
}

fn format_clock_time(timestamp_millis: i64) -> String {
    let Some(date_time_utc) = DateTime::<Utc>::from_timestamp_millis(timestamp_millis) else {
        return "--:--".to_string();
    };

    date_time_utc
        .with_timezone(&Local)
        .format("%H:%M")
        .to_string()
}

fn format_duration(duration_millis: i64) -> String {
    let total_seconds = duration_millis.max(0) / 1000;
    let minutes = total_seconds / 60;
    let seconds = total_seconds % 60;

    if minutes >= 60 {
        let hours = minutes / 60;
        let remaining_minutes = minutes % 60;
        format!("{hours}h {remaining_minutes}m")
    } else {
        format!("{minutes}:{seconds:02}")
    }
}
