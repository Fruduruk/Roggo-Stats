use std::sync::{Arc, Mutex};

use eframe::egui::{self, Context};
use uuid::Uuid;

use crate::core::{
    contract::{DetailedMatchDto, DetailedPlayerDto, DetailedPlayerStatsDto, DetailedTeamDto}, time::format_ms_min_seconds, ui::tasks
};

#[derive(Clone, Copy)]
struct LeaderboardPlayer<'a> {
    player: &'a DetailedPlayerDto,
    team: &'a DetailedTeamDto,
}

#[derive(Default)]
pub struct Content {
    pub detailed_match: Option<DetailedMatchDto>,
}

#[derive(Default)]
pub struct MatchUi {
    content: Arc<Mutex<Content>>,
}
impl MatchUi {
    pub fn reload(&self, context: Context, match_guid: Uuid) {
        tasks::load_detailed_match_by_id(context, self.content.clone(), match_guid);
    }

    pub fn ui(&self, ui: &mut egui::Ui) {
        let Ok(content) = self.content.lock() else {
            ui.label("Failed to lock match content.");
            return;
        };

        let Some(detailed_match) = &content.detailed_match else {
            ui.centered_and_justified(|ui| {
                ui.label("No match selected.");
            });
            return;
        };

        render_match_leaderboards(ui, detailed_match);
    }
}

fn render_match_leaderboards(ui: &mut egui::Ui, detailed_match: &DetailedMatchDto) {
    let players = collect_players(detailed_match);

    egui::ScrollArea::vertical().show(ui, |ui| {
        render_match_header(ui, detailed_match);

        ui.add_space(12.0);

        ui.heading("Classic Stats");

        render_number_leaderboard(ui, "Score", &players, |p| p.score);
        render_number_leaderboard(ui, "Goals", &players, |p| p.goals);
        render_number_leaderboard(ui, "Shots", &players, |p| p.shots);
        render_number_leaderboard(ui, "Assists", &players, |p| p.assists);
        render_number_leaderboard(ui, "Saves", &players, |p| p.saves);
        render_number_leaderboard(ui, "Demos", &players, |p| p.demos);

        ui.add_space(16.0);

        ui.heading("Detailed Stats");

        render_percent_leaderboard(ui, "Boosting", &players, |s| s.percent_boosting);
        render_percent_leaderboard(ui, "Supersonic", &players, |s| s.percent_supersonic);
        render_percent_leaderboard(ui, "On Ground", &players, |s| s.percent_on_ground);
        render_percent_leaderboard(ui, "On Wall", &players, |s| s.percent_on_wall);
        render_percent_leaderboard(ui, "Powersliding", &players, |s| s.percent_powersliding);
        render_percent_leaderboard(ui, "Demolished", &players, |s| s.percent_demolished);
    });
}
fn render_match_header(ui: &mut egui::Ui, detailed_match: &DetailedMatchDto) {
    ui.vertical_centered(|ui| {
        ui.heading(format!(
            "{} {} : {} {}",
            detailed_match.own_team.name,
            detailed_match.own_team.score,
            detailed_match.enemy_team.score,
            detailed_match.enemy_team.name
        ));

        ui.label(format!(
            "{} | {} {}",
            detailed_match.arena,
            format_ms_min_seconds(detailed_match.duration),
            if detailed_match.had_overtime {
                " | Overtime"
            } else {
                ""
            }
        ));
    });
}

fn collect_players<'a>(detailed_match: &'a DetailedMatchDto) -> Vec<LeaderboardPlayer<'a>> {
    detailed_match
        .own_team
        .players
        .iter()
        .map(|player| LeaderboardPlayer {
            player,
            team: &detailed_match.own_team,
        })
        .chain(
            detailed_match
                .enemy_team
                .players
                .iter()
                .map(|player| LeaderboardPlayer {
                    player,
                    team: &detailed_match.enemy_team,
                }),
        )
        .collect()
}fn render_number_leaderboard<F>(
    ui: &mut egui::Ui,
    title: &str,
    players: &[LeaderboardPlayer<'_>],
    value_fn: F,
) where
    F: Fn(&DetailedPlayerDto) -> i64,
{
    let mut entries: Vec<_> = players
        .iter()
        .map(|entry| (*entry, value_fn(entry.player)))
        .collect();

    entries.sort_by(|a, b| b.1.cmp(&a.1));

    egui::Frame::group(ui.style())
        .inner_margin(egui::Margin::same(8))
        .show(ui, |ui| {
            ui.set_width(ui.available_width());

            ui.strong(title);
            ui.add_space(4.0);

            egui::Grid::new(format!("leaderboard_{title}"))
                .num_columns(4)
                .spacing([12.0, 4.0])
                .striped(true)
                .show(ui, |ui| {
                    ui.small("#");
                    ui.small("Player");
                    ui.small("Team");
                    ui.small("Value");
                    ui.end_row();

                    for (index, (entry, value)) in entries.iter().enumerate() {
                        ui.label(format!("{}", index + 1));
                        ui.label(&entry.player.display_name);
                        ui.label(&entry.team.name);
                        ui.strong(value.to_string());
                        ui.end_row();
                    }
                });
        });

    ui.add_space(8.0);
}
fn render_percent_leaderboard<F>(
    ui: &mut egui::Ui,
    title: &str,
    players: &[LeaderboardPlayer<'_>],
    value_fn: F,
) where
    F: Fn(&DetailedPlayerStatsDto) -> f64,
{
    let mut entries: Vec<_> = players
        .iter()
        .filter_map(|entry| {
            entry
                .player
                .stats
                .as_ref()
                .map(|stats| (*entry, value_fn(stats)))
        })
        .collect();

    entries.sort_by(|a, b| {
        b.1.partial_cmp(&a.1)
            .unwrap_or(std::cmp::Ordering::Equal)
    });

    egui::Frame::group(ui.style())
        .inner_margin(egui::Margin::same(8))
        .show(ui, |ui| {
            ui.set_width(ui.available_width());

            ui.strong(title);
            ui.add_space(4.0);

            if entries.is_empty() {
                ui.small("No detailed stats available.");
                return;
            }

            egui::Grid::new(format!("percent_leaderboard_{title}"))
                .num_columns(4)
                .spacing([12.0, 4.0])
                .striped(true)
                .show(ui, |ui| {
                    ui.small("#");
                    ui.small("Player");
                    ui.small("Team");
                    ui.small("Value");
                    ui.end_row();

                    for (index, (entry, value)) in entries.iter().enumerate() {
                        let value = value.clamp(0.0, 1.0);
                        let percent = value * 100.0;

                        ui.label(format!("{}", index + 1));
                        ui.label(&entry.player.display_name);
                        ui.label(&entry.team.name);

                        let width = ui.available_width();
                        ui.add(
                            egui::ProgressBar::new(value as f32)
                                .desired_width(width)
                                .text(format!("{percent:.1}%")),
                        );

                        ui.end_row();
                    }
                });
        });

    ui.add_space(8.0);
}