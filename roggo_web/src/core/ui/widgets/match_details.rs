use crate::core::{
    icons,
    links::to_tracker_network_link,
    time::{format_ms_min_seconds, format_ms_time_without_seconds},
    ui::{
        components::tab_control::Tab, mappers::map_arena, theme::colors::colors,
        widgets::icon_button,
    },
};
use eframe::egui::{self, RichText};
use roggo_contract::*;

pub fn ui(
    ui: &mut egui::Ui,
    session_match_dto: Option<&SessionMatchDto>,
    new_tab: &mut Option<Tab>,
) {
    egui::Frame::new()
        .fill(colors(ui).panel)
        .corner_radius(5.0)
        .inner_margin(egui::Margin::symmetric(11, 10))
        .stroke(egui::Stroke::new(1.0, colors(ui).border))
        .show(ui, |ui| {
            ui.set_min_width(ui.available_width());
            ui.set_min_height(130.0);
            content(ui, session_match_dto, new_tab);
        });
}
fn content(
    ui: &mut egui::Ui,
    session_match_dto: Option<&SessionMatchDto>,
    new_tab: &mut Option<Tab>,
) {
    ui.horizontal(|ui| {
        if let Some(m) = session_match_dto {
            ui.vertical(|ui| {
                ui.spacing_mut().item_spacing.y = 1.0;
                ui.label(egui::RichText::new(map_arena(&m.arena)).size(14.0).strong());
                let time_string = format!(
                    "{} for {} min",
                    format_ms_time_without_seconds(m.created_at),
                    format_ms_min_seconds(m.duration)
                );
                ui.label(
                    egui::RichText::new(time_string)
                        .font(egui::FontId::new(10.0, egui::FontFamily::Proportional))
                        .color(ui.visuals().weak_text_color()),
                );
            });
        }

        ui.with_layout(egui::Layout::right_to_left(egui::Align::TOP), |ui| {
            let enabled = session_match_dto.is_some();

            if icon_button::ui(
                ui,
                icons::INSIDE.clone(),
                20.0,
                "Open match details",
                enabled,
            )
            .clicked()
            {
                *new_tab = Some(Tab::Match);
            }
        });
    });

    let Some(m) = session_match_dto else {
        return;
    };

    ui.add_space(8.0);

    let enemies = ui.link("Enemies").on_hover_text("Open all enemy profiles");

    if enemies.clicked() {
        for enemy in &m.enemies {
            open_tracker(ui, enemy);
        }
    }

    ui.indent("enemies", |ui| {
        for enemy in &m.enemies {
            let text = RichText::new(&enemy.display_name)
                .font(egui::FontId::new(
                    12.0,
                    egui::FontFamily::Name("player_name".into()),
                ))
                .color(colors(ui).accent);

            if ui.link(text).clicked() {
                open_tracker(ui, enemy);
            }
        }
    });
}

fn open_tracker(ui: &egui::Ui, player: &PlayerDto) {
    if let Some(url) = to_tracker_network_link(&player.primary_id, &player.display_name) {
        ui.open_url(egui::OpenUrl { url, new_tab: true });
    }
}
