use eframe::egui::{self, FontFamily, ImageSource, Pos2, Rect, RichText, Vec2, pos2, vec2};
use egui::{Color32, Mesh, Shape};
use itertools::Itertools;

use crate::core::{
    contract::{MVPType, session::SessionMatchDto},
    icons,
    links::to_tracker_network_link,
    time::{format_ms_min_seconds, format_ms_time_without_seconds},
    ui::{components::tab_control::Tab, mappers::map_arena, theme::colors::colors},
};

pub fn ui(
    ui: &mut egui::Ui,
    rect: Rect,
    id: egui::Id,
    session_match_dto: &SessionMatchDto,
) -> egui::Response {
    let response = ui.interact(rect, id, egui::Sense::click());

    let color = match session_match_dto.won {
        Some(true) => colors(ui).success,
        Some(false) => colors(ui).error,
        None => colors(ui).warning,
    }
    .gamma_multiply(0.45);

    // let color = if response.hovered() {
    //     color.gamma_multiply(0.5)
    // } else {
    //     color
    // };

    ui.painter().rect_filled(rect, 5.0, color);

    // let hover_text = get_hover_text(session_match_dto);

    // response.clone().on_hover_text_at_pointer(
    //     RichText::new(hover_text).family(FontFamily::Name("player_name".into())),
    // );

    let inside_button_rect =
        Rect::from_min_size(rect.right_top() - vec2(15.0, 0.0), vec2(15.0, 15.0));

    // let mut card_ui = ui.new_child(
    //     egui::UiBuilder::new()
    //         .id_salt(id.with("Inside Button"))
    //         .max_rect(inside_button_rect)
    //         .layout(egui::Layout::left_to_right(egui::Align::Center)),
    // );

    // let inside_button_response = custom_button(&mut card_ui, icons::INSIDE.clone());

    // if inside_button_response.hovered() {
    //     inside_button_response.clone().on_hover_text_at_pointer("open match details");
    // }

    // if inside_button_response.clone().clicked() {
    //     *selected_tab = Tab::Match;
    // }

    // let inside_button_rect =
    //     Rect::from_min_size(rect.right_bottom() - vec2(15.0, 15.0), vec2(15.0, 15.0));


    // let mut card_ui = ui.new_child(
    //     egui::UiBuilder::new()
    //         .id_salt(id.with("Tracker Button"))
    //         .max_rect(inside_button_rect)
    //         .layout(egui::Layout::left_to_right(egui::Align::Center)),
    // );

    // let tracker_button_response = custom_button(&mut card_ui, icons::QUESTION_MARK.clone());

    // if tracker_button_response.hovered() {
    //     tracker_button_response.clone().on_hover_text_at_pointer("open Tracker Network profiles for enemies");
    // }

    // if tracker_button_response.clicked() {
    //     for enemy in &session_match_dto.enemies {
    //         if let Some(url) = to_tracker_network_link(&enemy.primary_id, &enemy.display_name) {
    //             ui.open_url(egui::OpenUrl { url, new_tab: true });
    //         }
    //     }
    // }

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
        let mvp_rect =
            egui::Rect::from_min_size(rect.left_top() + vec2(0.0, 0.0), mvp_icon_size);

        egui::Image::new(icons::STAR)
            .fit_to_exact_size(mvp_icon_size)
            .tint(colors(ui).warning)
            .paint_at(ui, mvp_rect);
    }

    // let full_response = response
    //     .union(inside_button_response)
    //     .union(tracker_button_response);

    let border_color = if response.hovered() {
        colors(ui).on_panel.gamma_multiply(0.7)
    } else {
        colors(ui).on_panel.gamma_multiply(0.2)
    };

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

    // let border_color = if response.hovered() {
    //     egui::Color32::WHITE
    // } else {
    //     egui::Color32::from_gray(90)
    // };

    // ui.painter().rect_stroke(
    //     rect,
    //     3.0,
    //     egui::Stroke::new(
    //         1.0, // Randdicke
    //         border_color,
    //     ),
    //     egui::StrokeKind::Inside,
    // );

    let icon_rect = egui::Rect::from_center_size(rect.center(), size);

    egui::Image::new(image_source)
        .fit_to_exact_size(size)
        .tint(icon_color)
        .paint_at(ui, icon_rect);

    response
}
