use eframe::egui;

use crate::core::{contract::DaySessionDto, ui::theme::colors::colors};

pub fn ui(ui: &mut egui::Ui, session: &DaySessionDto) -> egui::Response {
    egui::Frame::new()
        .corner_radius(5.0)
        .fill(colors(ui).surface_alt)
        .inner_margin(5.0)
        .show(ui, |ui| {
            let DaySessionDto {
                playlist,
                created_at,
                ended_at,
                session_type,
                matches,
            } = &session;



            
            ui.heading(playlist.to_string())
            
        })
        .inner
}
