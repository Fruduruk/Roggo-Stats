use std::sync::{Arc, Mutex};

use eframe::egui::{
    self, Align2, Color32, Context, FontId, RichText, Sense, Stroke, Vec2,
};
use uuid::Uuid;

use crate::core::{
    contract::{
        DetailedAverageAdvancedStatsDto, DetailedAverageCoreStatsDto,
        DetailedAveragePlayerDto, DetailedSessionDto,
    },
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

                self.render_core_stats(ui, session, player_name);

                ui.add_space(18.0);
                ui.separator();
                ui.add_space(12.0);

                self.render_advanced_stats(ui, session, player_name);
            });
    }

    fn render_core_stats(
        &self,
        ui: &mut egui::Ui,
        session: &DetailedSessionDto,
        player_name: &str,
    ) {
        ui.heading("Core Stats");

        let enemy_average = session.average_enemy_core_stats.as_ref();

        let own_player = session
            .own_team_player_averages
            .iter()
            .find(|p| p.username == player_name);

        if let Some(player) = own_player {
            self.render_core_player_card(ui, "You", player, enemy_average);
            ui.add_space(8.0);
        }

        let mates: Vec<_> = session
            .own_team_player_averages
            .iter()
            .filter(|p| p.username != player_name)
            .collect();

        if !mates.is_empty() {
            ui.label(RichText::new("Mates").strong());

            for mate in mates {
                self.render_core_player_card(ui, &mate.username, mate, enemy_average);
                ui.add_space(6.0);
            }
        }

        if let Some(team_average) = &session.average_team_player_core_stats {
            ui.add_space(10.0);
            ui.label(RichText::new("Team mate average").strong());

            egui::Frame::group(ui.style()).show(ui, |ui| {
                self.render_core_grid(ui, team_average, enemy_average);
            });
        }
    }

    fn render_core_player_card(
        &self,
        ui: &mut egui::Ui,
        title: &str,
        player: &DetailedAveragePlayerDto,
        enemy_average: Option<&DetailedAverageCoreStatsDto>,
    ) {
        egui::Frame::group(ui.style()).show(ui, |ui| {
            ui.label(RichText::new(title).strong());
            ui.add_space(4.0);

            self.render_core_grid(ui, &player.average_core_stats, enemy_average);
        });
    }

    fn render_core_grid(
        &self,
        ui: &mut egui::Ui,
        stats: &DetailedAverageCoreStatsDto,
        enemy_average: Option<&DetailedAverageCoreStatsDto>,
    ) {
        egui::Grid::new(ui.next_auto_id())
            .num_columns(3)
            .spacing([18.0, 4.0])
            .show(ui, |ui| {
                self.core_row(
                    ui,
                    "Score",
                    stats.average_score,
                    enemy_average.map(|e| e.average_score),
                    1,
                );
                self.core_row(
                    ui,
                    "Goals",
                    stats.average_goals,
                    enemy_average.map(|e| e.average_goals),
                    2,
                );
                self.core_row(
                    ui,
                    "Shots",
                    stats.average_shots,
                    enemy_average.map(|e| e.average_shots),
                    2,
                );
                self.core_row(
                    ui,
                    "Assists",
                    stats.average_assists,
                    enemy_average.map(|e| e.average_assists),
                    2,
                );
                self.core_row(
                    ui,
                    "Saves",
                    stats.average_saves,
                    enemy_average.map(|e| e.average_saves),
                    2,
                );
                self.core_row(
                    ui,
                    "Demos",
                    stats.average_demos,
                    enemy_average.map(|e| e.average_demos),
                    2,
                );
            });
    }

    fn core_row(
        &self,
        ui: &mut egui::Ui,
        label: &str,
        value: f64,
        enemy_value: Option<f64>,
        decimals: usize,
    ) {
        ui.label(label);
        ui.monospace(format!("{value:.decimals$}"));

        if let Some(enemy_value) = enemy_value {
            let diff = value - enemy_value;
            let color = if diff >= 0.0 {
                Color32::from_rgb(70, 170, 90)
            } else {
                Color32::from_rgb(210, 80, 80)
            };

            ui.label(
                RichText::new(format!("({diff:+.1})"))
                    .color(color)
                    .monospace(),
            );
        } else {
            ui.label(RichText::new("(enemy n/a)").weak());
        }

        ui.end_row();
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

        self.advanced_chart(
            ui,
            "Boosting %",
            &players,
            team_average.as_ref(),
            |s| s.average_percent_boosting,
        );

        self.advanced_chart(
            ui,
            "Demolished %",
            &players,
            team_average.as_ref(),
            |s| s.average_percent_demolished,
        );

        self.advanced_chart(
            ui,
            "On ground %",
            &players,
            team_average.as_ref(),
            |s| s.average_percent_on_ground,
        );

        self.advanced_chart(
            ui,
            "On wall %",
            &players,
            team_average.as_ref(),
            |s| s.average_percent_on_wall,
        );

        self.advanced_chart(
            ui,
            "Powersliding %",
            &players,
            team_average.as_ref(),
            |s| s.average_percent_powersliding,
        );

        self.advanced_chart(
            ui,
            "Supersonic %",
            &players,
            team_average.as_ref(),
            |s| s.average_percent_supersonic,
        );
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
            .map(|p| {
                (
                    p.label.as_str(),
                    value_selector(p.stats),
                    p.color,
                )
            })
            .collect();

        if let Some(team_average) = team_average {
            bars.push((
                team_average.label.as_str(),
                value_selector(team_average.stats),
                team_average.color,
            ));
        }

        let max_value = bars
            .iter()
            .map(|(_, value, _)| *value)
            .fold(0.0_f64, f64::max)
            .max(1.0);

        let desired_size = Vec2::new(ui.available_width(), 180.0);
        let (rect, _) = ui.allocate_exact_size(desired_size, Sense::hover());
        let painter = ui.painter_at(rect);

        let left_padding = 42.0;
        let bottom_padding = 34.0;
        let top_padding = 10.0;
        let right_padding = 12.0;

        let chart_left = rect.left() + left_padding;
        let chart_right = rect.right() - right_padding;
        let chart_top = rect.top() + top_padding;
        let chart_bottom = rect.bottom() - bottom_padding;

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

        painter.text(
            egui::pos2(chart_left - 6.0, chart_top),
            Align2::RIGHT_TOP,
            format!("{max_value:.1}"),
            FontId::monospace(11.0),
            Color32::GRAY,
        );

        painter.text(
            egui::pos2(chart_left - 6.0, chart_bottom),
            Align2::RIGHT_BOTTOM,
            "0",
            FontId::monospace(11.0),
            Color32::GRAY,
        );

        let bar_count = bars.len().max(1) as f32;
        let chart_width = chart_right - chart_left;
        let slot_width = chart_width / bar_count;
        let bar_width = slot_width * 0.55;
        let chart_height = chart_bottom - chart_top;

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
                format!("{value:.1}"),
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