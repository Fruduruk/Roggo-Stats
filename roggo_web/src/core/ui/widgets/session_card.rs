use eframe::egui::{self};
use jiff::{Timestamp, Zoned, tz::TimeZone};

use crate::core::{
    contract::{DaySessionDto, SessionTypeDto},
    ui::theme::colors::colors,
};

pub fn ui(ui: &mut egui::Ui, session: &DaySessionDto) -> egui::Response {
    let won = session.matches.iter().filter(|m| m.won).count();
    let lost = session.matches.len() - won;

    let duration_minutes = (session.ended_at - session.created_at) / 60_000;

    let win_color = if won > lost {
        colors(ui).success
    } else if won == lost {
        egui::Color32::ORANGE
    } else {
        colors(ui).error
    };

    let mut frame = egui::Frame::new()
        .corner_radius(5.0)
        .fill(colors(ui).surface_alt)
        .inner_margin(10.0)
        .stroke(egui::Stroke::new(1.0, colors(ui).border))
        .begin(ui);

    {
        let ui = &mut frame.content_ui;

        header(session, ui);
        ui.add_space(2.0);
        center(session, won, win_color, ui);
        ui.add_space(8.0);
        ui.separator();
        ui.add_space(6.0);

        footer(session, duration_minutes, ui);
    }

    let response = frame.allocate_space(ui);

    if response.hovered() {
        frame.frame.fill = colors(ui).surface_hover
    }

    frame.paint(ui);

    response.interact(egui::Sense::click())
}

fn footer(session: &DaySessionDto, duration_minutes: i64, ui: &mut egui::Ui) {
    ui.horizontal(|ui| {
        ui.label(format!("{duration_minutes} min"));

        ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
            ui.label(format!(
                "{} - {}",
                ms_to_time_string(session.created_at),
                ms_to_time_string(session.ended_at)
            ));
        });
    });
}

fn center(session: &DaySessionDto, won: usize, win_color: egui::Color32, ui: &mut egui::Ui) {
    ui.horizontal(|ui| {
        match &session.session_type {
            SessionTypeDto::Solo => {
                ui.label(egui::RichText::new("Solo").color(colors(ui).text_weak));
            }

            SessionTypeDto::Team(players) => {
                ui.label(egui::RichText::new("Team").color(colors(ui).text_weak));

                ui.label(
                    players
                        .iter()
                        .map(|p| p.display_name.as_str())
                        .collect::<Vec<_>>()
                        .join(", "),
                );
            }
        }
        ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
            ui.label(
                egui::RichText::new(format!("{} / {}", won, session.matches.len()))
                    .color(colors(ui).text_weak.blend(win_color.gamma_multiply(0.5))),
            );
        });
    });
}

fn header(session: &DaySessionDto, ui: &mut egui::Ui) {
    ui.horizontal(|ui| {
        ui.label(
            egui::RichText::new(session.playlist.to_string())
                .size(16.0)
                .strong(),
        );
        ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
            ui.spacing_mut().item_spacing.x = 0.0;

            for game_won in session.matches.iter().map(|g| g.won).rev() {
                let fill = if game_won {
                    colors(ui).success
                } else {
                    colors(ui).error
                };
                egui::Frame::new()
                    .corner_radius(4.0)
                    .fill(fill)
                    .outer_margin(egui::Margin::symmetric(1, 3))
                    .inner_margin(egui::Margin::symmetric(1, 3))
                    .show(ui, |ui| {
                        ui.set_min_size(egui::vec2(5.0, 10.0));
                    });
            }
        });
    });
}

fn ms_to_time_string(ms: i64) -> String {
    Zoned::new(Timestamp::from_millisecond(ms).unwrap(), TimeZone::system())
        .strftime("%H:%M")
        .to_string()
}
