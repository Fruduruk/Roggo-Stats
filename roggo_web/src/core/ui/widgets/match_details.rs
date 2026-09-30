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

}