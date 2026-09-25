use eframe::egui::{
    self,  FontFamily, ImageSource, Pos2, Rect, RichText, Vec2, pos2, vec2,
};
use egui::{Color32, Mesh, Shape};
use itertools::Itertools;

use crate::core::{
    contract::{MVPType, session::SessionMatchDto},
    icons,
    links::to_tracker_network_link,
    ui::{components::tab_control::Tab, mappers::map_arena, theme::colors::colors},
};
fn paint_custom_mesh(
    ui: &egui::Ui,
    rect: egui::Rect,
    left_color: egui::Color32,
    right_color: egui::Color32,
    radius: f32,
) {
    fn lerp_color(left: Color32, right: Color32, t: f32) -> Color32 {
        Color32::from_rgba_premultiplied(
            egui::lerp(left.r() as f32..=right.r() as f32, t) as u8,
            egui::lerp(left.g() as f32..=right.g() as f32, t) as u8,
            egui::lerp(left.b() as f32..=right.b() as f32, t) as u8,
            egui::lerp(left.a() as f32..=right.a() as f32, t) as u8,
        )
    }

    let radius = radius.min(rect.width() / 2.0).min(rect.height() / 2.0);

    let segments = 8;

    let corners = [
        (
            Pos2::new(rect.left() + radius, rect.top() + radius),
            std::f32::consts::PI,
        ),
        (
            Pos2::new(rect.right() - radius, rect.top() + radius),
            -std::f32::consts::FRAC_PI_2,
        ),
        (
            Pos2::new(rect.right() - radius, rect.bottom() - radius),
            0.0,
        ),
        (
            Pos2::new(rect.left() + radius, rect.bottom() - radius),
            std::f32::consts::FRAC_PI_2,
        ),
    ];

    let mut mesh = Mesh::default();

    let center = rect.center();
    mesh.colored_vertex(center, lerp_color(left_color, right_color, 0.5));

    for (corner_center, start_angle) in corners {
        for i in 0..=segments {
            let t = i as f32 / segments as f32;

            let angle = start_angle + t * std::f32::consts::FRAC_PI_2;

            let point = corner_center + Vec2::new(angle.cos(), angle.sin()) * radius;

            let color_t = (point.x - rect.left()) / rect.width();

            let color = lerp_color(left_color, right_color, color_t);

            mesh.colored_vertex(point, color);
        }
    }

    let outer_vertices = mesh.vertices.len() - 1;

    for i in 0..outer_vertices {
        let current = (i + 1) as u32;
        let next = ((i + 1) % outer_vertices + 1) as u32;

        mesh.add_triangle(0, current, next);
    }

    ui.painter().add(Shape::mesh(mesh));
}
pub fn ui(
    ui: &mut egui::Ui,
    rect: Rect,
    id: egui::Id,
    session_match_dto: &SessionMatchDto,
    selected_tab: &mut Tab,
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

    if let MVPType::MVP | MVPType::ACE = session_match_dto.mvp_type {
        paint_custom_mesh(ui, rect, colors(ui).accent, color, 5.0);
    } else {
        ui.painter().rect_filled(rect, 5.0, color);
    }

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

    let hover_text = format!(
        "Arena: {}\nAllies:\n   {}\nEnemies:\n   {}",
        map_arena(&session_match_dto.arena),
        ally_string,
        enemy_string
    );

    response
        .clone()
        .on_hover_text_at_pointer(RichText::new(hover_text).family(FontFamily::Name("player_name".into())));

    let border_color = if response.hovered() {
        colors(ui).on_panel.gamma_multiply(0.7)
    } else {
        colors(ui).on_panel.gamma_multiply(0.2)
    };

    ui.painter().rect_stroke(
        rect,
        5.0,
        egui::Stroke::new(
            1.0,
            border_color,
        ),
        egui::StrokeKind::Inside,
    );

    let inside_button_rect =
        Rect::from_min_size(rect.right_top() - vec2(15.0, 0.0), vec2(15.0, 15.0));

    let mut card_ui = ui.new_child(
        egui::UiBuilder::new()
            .id_salt(id.with("Inside Button"))
            .max_rect(inside_button_rect)
            .layout(egui::Layout::left_to_right(egui::Align::Center)),
    );

    let button_response = custom_button(&mut card_ui, icons::INSIDE.clone());

    if button_response.clicked() {
        *selected_tab = Tab::Match;
    }

    let inside_button_rect =
        Rect::from_min_size(rect.right_bottom() - vec2(15.0, 15.0), vec2(15.0, 15.0));

    let mut card_ui = ui.new_child(
        egui::UiBuilder::new()
            .id_salt(id.with("Tracker Button"))
            .max_rect(inside_button_rect)
            .layout(egui::Layout::left_to_right(egui::Align::Center)),
    );

    let button_response = custom_button(&mut card_ui, icons::SMURF.clone());

    if button_response.clicked() {
        for enemy in &session_match_dto.enemies {
            if let Some(url) = to_tracker_network_link(&enemy.primary_id, &enemy.display_name) {
                ui.open_url(egui::OpenUrl { url, new_tab: true });
            }
        }
    }

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

    response
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
