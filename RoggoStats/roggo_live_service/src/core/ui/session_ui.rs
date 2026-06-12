use std::sync::{Arc, Mutex};

use eframe::egui::{self, Align2, Color32, Context, FontId, RichText, Sense, Stroke, Vec2};
use uuid::Uuid;

use crate::core::{
    contract::{DetailedAverageAdvancedStatsDto, DetailedAverageCoreStatsDto, DetailedSessionDto},
    ui::tasks,
};

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
        tasks::load_detailed_session(context, self.content.clone(), match_guids);
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

        let desired_size = Vec2::new(ui.available_width(), 96.0);
        let (rect, _) = ui.allocate_exact_size(desired_size, Sense::hover());
        let painter = ui.painter_at(rect);

        let left_padding = 54.0;
        let right_padding = 12.0;
        let top_padding = 24.0;
        let bottom_padding = 28.0;

        let timeline_left = rect.left() + left_padding;
        let timeline_right = rect.right() - right_padding;
        let timeline_top = rect.top() + top_padding;
        let timeline_bottom = rect.bottom() - bottom_padding;
        let timeline_y = (timeline_top + timeline_bottom) / 2.0;

        let timeline_width = timeline_right - timeline_left;
        let bar_height = 18.0;

        painter.line_segment(
            [
                egui::pos2(timeline_left, timeline_y),
                egui::pos2(timeline_right, timeline_y),
            ],
            Stroke::new(1.0, Color32::GRAY),
        );

        painter.text(
            egui::pos2(timeline_left, rect.bottom() - 6.0),
            Align2::LEFT_BOTTOM,
            format_time_offset(0),
            FontId::monospace(11.0),
            Color32::GRAY,
        );

        painter.text(
            egui::pos2(timeline_right, rect.bottom() - 6.0),
            Align2::RIGHT_BOTTOM,
            format_time_offset(last_ended_at - first_started_at),
            FontId::monospace(11.0),
            Color32::GRAY,
        );

        for session_match in visible_matches {
            let start_offset = (session_match.created_at - first_started_at).max(0) as f32;
            let end_offset = (session_match.ended_at - first_started_at).max(0) as f32;

            let x1 = timeline_left + timeline_width * (start_offset / total_duration);
            let x2 = timeline_left + timeline_width * (end_offset / total_duration);

            let min_width = 3.0;
            let bar_right = x2.max(x1 + min_width);

            let color = match session_match.won {
                Some(true) => Color32::from_rgb(80, 180, 110),
                Some(false) => Color32::from_rgb(210, 80, 80),
                None => Color32::from_rgb(130, 130, 130),
            };

            let bar_rect = egui::Rect::from_min_max(
                egui::pos2(x1, timeline_y - bar_height / 2.0),
                egui::pos2(bar_right, timeline_y + bar_height / 2.0),
            );

            painter.rect_filled(bar_rect, 3.0, color);
        }

        ui.horizontal(|ui| {
            timeline_legend(ui, Color32::from_rgb(80, 180, 110), "Won");
            timeline_legend(ui, Color32::from_rgb(210, 80, 80), "Lost");
            timeline_legend(ui, Color32::from_rgb(130, 130, 130), "Unknown");
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

fn format_time_offset(seconds: i64) -> String {
    let seconds = seconds.max(0);
    let minutes = seconds / 60;
    let remaining_seconds = seconds % 60;

    if minutes >= 60 {
        let hours = minutes / 60;
        let remaining_minutes = minutes % 60;
        format!("{hours}h {remaining_minutes}m")
    } else {
        format!("{minutes}:{remaining_seconds:02}")
    }
}
