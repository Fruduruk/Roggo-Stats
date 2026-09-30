use eframe::egui::{self, ImageSource, Rect, vec2};
use itertools::Itertools;
use uuid::Uuid;

use crate::core::{
    contract::{MVPType, session::SessionMatchDto},
    icons,
    time::{format_ms_min_seconds, format_ms_time_without_seconds},
    ui::{mappers::map_arena, theme::colors::colors},
};

pub fn ui(
    ui: &mut egui::Ui,
    rect: Rect,
    id: egui::Id,
    session_match_dto: &SessionMatchDto,
    selected_match_guid: &Option<Uuid>,
) -> egui::Response {
    let response = ui.interact(rect, id, egui::Sense::click());

    let color = match session_match_dto.won {
        Some(true) => colors(ui).success,
        Some(false) => colors(ui).error,
        None => colors(ui).warning,
    };

    let (color, border_color) =
        if response.hovered() || &Some(session_match_dto.match_guid) == selected_match_guid {
            (color.gamma_multiply(0.8), colors(ui).on_panel.gamma_multiply(0.7))
        } else {
            (
                color.gamma_multiply(0.5),
                colors(ui).on_panel.gamma_multiply(0.2),
            )
        };

    let rect = if response.is_pointer_button_down_on() {
        rect.expand(-1.0)
    } else {
        rect
    };

    ui.painter().rect_filled(rect, 5.0, color);

    ui.painter().text(
        rect.center(),
        egui::Align2::CENTER_CENTER,
        format!(
            "{} : {}",
            session_match_dto.own_score, session_match_dto.enemy_score
        ),
        egui::FontId::default(),
        colors(ui).on_panel,
    );

    if MVPType::MVP == session_match_dto.mvp_type || MVPType::ACE == session_match_dto.mvp_type {
        let mvp_icon_size = vec2(20.0, 20.0);
        let mvp_rect = egui::Rect::from_min_size(rect.left_top() + vec2(0.0, 0.0), mvp_icon_size);

        egui::Image::new(icons::STAR)
            .fit_to_exact_size(mvp_icon_size)
            .tint(colors(ui).warning)
            .paint_at(ui, mvp_rect);
    }

    ui.painter().rect_stroke(
        rect,
        5.0,
        egui::Stroke::new(1.0, border_color),
        egui::StrokeKind::Inside,
    );
    response
}

fn get_hover_text(session_match_dto: &SessionMatchDto) -> String {
    let enemy_string = session_match_dto
        .enemies
        .iter()
        .map(|p| p.display_name.clone())
        .join("\n   ");

    let ally_string = session_match_dto
        .allies
        .iter()
        .map(|p| p.display_name.clone())
        .join("\n   ");

    let time_string = format!(
        "{} for {}",
        format_ms_time_without_seconds(session_match_dto.created_at),
        format_ms_min_seconds(session_match_dto.duration)
    );
    let hover_text = format!(
        "{}\nArena: {}\nAllies:\n   {}\nEnemies:\n   {}",
        time_string,
        map_arena(&session_match_dto.arena),
        ally_string,
        enemy_string
    );
    hover_text
}

pub fn custom_button(ui: &mut egui::Ui, image_source: ImageSource) -> egui::Response {
    let size = egui::vec2(15.0, 15.0);

    let (rect, response) = ui.allocate_exact_size(size, egui::Sense::click());

    let icon_color = if response.hovered() {
        colors(ui).on_panel.gamma_multiply(0.5)
    } else {
        colors(ui).on_panel
    };

    let icon_color = if response.is_pointer_button_down_on() {
        colors(ui).on_panel
    } else {
        icon_color
    };

    let icon_rect = egui::Rect::from_center_size(rect.center(), size);

    egui::Image::new(image_source)
        .fit_to_exact_size(size)
        .tint(icon_color)
        .paint_at(ui, icon_rect);

    response
}
