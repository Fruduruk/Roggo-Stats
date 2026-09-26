use eframe::egui::{self, Pos2, Rect, Vec2, vec2};

use crate::core::{
    contract::session::{SessionDto, SessionMatchDto}, ui::{
        components::tab_control::Tab, theme::colors::colors, widgets::{match_card, session_card},
    },
};

pub fn ui(ui: &mut egui::Ui, session: &SessionDto, selected_tab: &mut Tab) -> egui::Response {
    let match_card_min_width = 100.0;

    let Some(min_pixel_per_ms) = calculate_pixel_per_ms(ui, match_card_min_width, &session.matches)
    else {
        return ui.label("Not enough space for timeline");
    };

    let Some((match_card_rects, rows)) =
        calculate_match_card_rects_timeline(ui, min_pixel_per_ms, &session.matches)
    else {
        return ui.label("Match doesn't fit into available space");
    };

    let bounding = match_card_rects
        .iter()
        .copied()
        .fold(Rect::NOTHING, Rect::union);

    let desired_size = vec2(ui.available_width(), bounding.height());

    let (timeline_rect, timeline_response) =
        ui.allocate_exact_size(desired_size, egui::Sense::click());
    // let painter = ui.painter_at(timeline_rect);

    // painter.rect_stroke(
    //     timeline_rect,
    //     0.0,
    //     egui::Stroke::new(1.0, colors(ui).accent),
    //     egui::StrokeKind::Inside,
    // );
    for (session_match, rect) in session.matches.iter().zip(match_card_rects) {
        // let mut child_ui = ui.new_child(egui::UiBuilder::new().max_rect(rect).layout(*ui.layout()));
        match_card::ui(
            ui,
            rect,
            ui.id().with(format!("{rect}")),
            session_match,
            selected_tab,
        );
    }
    // painter.line_segment(
    //     [timeline_rect.left_center(), timeline_rect.right_center()],
    //     egui::Stroke::new(2.0, egui::Color32::GRAY),
    // );

    // let (Some(first), Some(last)) = (session.matches.first(), session.matches.last()) else {
    //     return timeline_response;
    // };

    timeline_response
}
fn calculate_match_card_rects_timeline(
    ui: &egui::Ui,
    pixel_per_ms: f32,
    matches: &[SessionMatchDto],
) -> Option<(Vec<Rect>, i32)> {
    let card_height = 32.0;
    let card_spacing = 5.0;
    let starting_pos = ui.cursor().left_top();
    let available_width = ui.available_width();
    let max_x = starting_pos.x + available_width;

    let mut current_row = 0;
    let mut row_start_ms = matches.first()?.created_at;

    let match_rects = matches
        .iter()
        .map(|sm| {
            let width = (sm.ended_at - sm.created_at) as f32 * pixel_per_ms;

            if width > available_width {
                return None;
            }

            let time_since_row_start = (sm.created_at - row_start_ms) as f32;

            let mut starting_x = starting_pos.x + time_since_row_start * pixel_per_ms;

            if starting_x + width > max_x {
                current_row += 1;

                row_start_ms = sm.created_at;
                starting_x = starting_pos.x;
            }

            let starting_y = starting_pos.y + current_row as f32 * (card_height + card_spacing);

            Some(Rect::from_min_size(
                Pos2::new(starting_x, starting_y),
                Vec2::new(width, card_height),
            ))
        })
        .collect::<Option<Vec<_>>>()?;

    Some((match_rects, current_row + 1))
}

fn calculate_pixel_per_ms(
    ui: &mut egui::Ui,
    min_width: f32,
    matches: &[SessionMatchDto],
) -> Option<f32> {
    let match_lengths = matches
        .iter()
        .map(|m| m.ended_at - m.created_at)
        .collect::<Vec<_>>();

    let (Some(shortest_match), Some(longest_match)) =
        (match_lengths.iter().min(), match_lengths.iter().max())
    else {
        return None;
    };

    let min_pixel_per_ms = min_width / *shortest_match as f32;
    let max_pixel_per_ms = ui.available_width() / *longest_match as f32;

    if min_pixel_per_ms <= max_pixel_per_ms {
        Some(min_pixel_per_ms)
    } else {
        None
    }
}
