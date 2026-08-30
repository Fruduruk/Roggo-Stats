use eframe::egui;

use crate::core::{
    contract::{DaySessionDto, SessionTypeDto},
    ui::theme::colors::colors,
};

pub fn ui(ui: &mut egui::Ui, session: &DaySessionDto) -> egui::Response {
    let won = session.matches.iter().filter(|m| m.won).count();
    let lost = session.matches.len() - won;

    let duration_minutes = (session.ended_at - session.created_at) / 60_000;

    egui::Frame::new()
        .corner_radius(5.0)
        .fill(colors(ui).surface_alt)
        .inner_margin(10.0)
        .show(ui, |ui| {
            ui.set_width(ui.available_width());

            // Header
            ui.horizontal(|ui| {
                ui.heading(session.playlist.to_string());

                ui.with_layout(
                    egui::Layout::right_to_left(egui::Align::Center),
                    |ui| {
                        ui.label(
                            egui::RichText::new(format!("{won}W  {lost}L"))
                                .strong(),
                        );
                    },
                );
            });

            ui.add_space(2.0);

            // Session info
            ui.horizontal(|ui| {
                match &session.session_type {
                    SessionTypeDto::Solo => {
                        ui.label(
                            egui::RichText::new("Solo")
                                .color(colors(ui).text_weak),
                        );
                    }

                    SessionTypeDto::Team(players) => {
                        ui.label(
                            egui::RichText::new("Team")
                                .color(colors(ui).text_weak),
                        );

                        ui.label(
                            players
                                .iter()
                                .map(|p| p.display_name.as_str())
                                .collect::<Vec<_>>()
                                .join(", "),
                        );
                    }
                }
            });

            ui.add_space(8.0);
            ui.separator();
            ui.add_space(6.0);

            // Summary
            ui.horizontal(|ui| {
                ui.label(format!(
                    "{} Matches",
                    session.matches.len()
                ));

                ui.label("·");

                ui.label(format!("{duration_minutes} min"));
            });

            ui.add_space(8.0);

            // Individual matches
            for game in &session.matches {
                    let text = format!(
                        "{} : {}",
                        game.own_score,
                        game.enemy_score
                    );

                    let fill = if game.won {
                        colors(ui).success
                    } else {
                        colors(ui).error
                    };

                    egui::Frame::new()
                        .corner_radius(4.0)
                        .fill(fill)
                        .inner_margin(egui::Margin::symmetric(7, 3))
                        .show(ui, |ui| {
                            ui.label(
                                egui::RichText::new(text)
                                    .strong(),
                            );
                        });
                }
        })
        .response
}