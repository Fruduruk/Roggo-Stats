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
    session_match_dto: &SessionMatchDto,
    selected_tab: &mut Tab,
)  {
    
    // let inside_button_rect =
    //     Rect::from_min_size(rect.right_top() - vec2(15.0, 0.0), vec2(15.0, 15.0));

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
}