use eframe::egui::{self, Pos2, Rect, Vec2};

use crate::core::{
    contract::session::{SessionDto, SessionMatchDto},
    ui::{
        components::tab_control::Tab,
        widgets::{match_card, session_card},
    },
};

pub fn ui(ui: &mut egui::Ui, session: &SessionDto, selected_tab: &mut Tab) -> egui::Response {
    let match_card_min_width = 100.0;

    let match_card_rects = calculate_match_card_rects(ui, match_card_min_width, &session.matches);

    let desired_size = egui::vec2(ui.available_width(), 80.0);
    let (timeline_rect, timeline_response) =
        ui.allocate_exact_size(desired_size, egui::Sense::click());
    let painter = ui.painter_at(timeline_rect);

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

fn get_smallest_pause_ms(matches: &[SessionMatchDto]) -> i64 {
    matches
        .windows(2)
        .map(|pair| pair[1].created_at - pair[0].ended_at)
        .min()
        .unwrap_or(2)
}

fn calculate_match_card_rects_timeline(
    ui: &egui::Ui,
    iteration: i64,
    match_card_min_width: f32,
    matches: &[SessionMatchDto],
    smallest_pause_ms: i64,
) -> Option<Vec<Rect>> {
    let (Some(first), Some(last)) = (matches.first(), matches.last()) else {
        return Some(vec![]);
    };

    let first_ms = first.created_at;
    let total_ms = last.ended_at - first.created_at;

    let pixel_per_ms =
        ui.available_width() / (total_ms - iteration * (smallest_pause_ms / 8)) as f32;
    let mut pause_deletion_offset = 0.0;
    let mut current_row = 0;

    let match_rects = matches
        .iter()
        .map(|sm| {
            let width = (sm.ended_at - sm.created_at) as f32 * pixel_per_ms;

            if width < match_card_min_width {
                return None;
            }

            let time_since_first = (sm.created_at - first_ms) as f32;

            let starting_pos = ui.cursor().left_top();

            let mut starting_x = starting_pos.x + time_since_first * pixel_per_ms;

            let mut starting_y = starting_pos.y;

            let max_x = ui.cursor().left() + ui.available_width();

            if starting_x > max_x {
                let offset_from_start = starting_x - starting_pos.x;
                let row = (offset_from_start / ui.available_width()).floor();
                starting_y += (32.0 + 5.0) * row; // calc line

                let actual_starting_x = starting_x - (ui.available_width() * row); // shift left

                if current_row < row as i32 {
                    pause_deletion_offset = actual_starting_x - starting_pos.x;
                    current_row = row as i32;
                }
                starting_x = actual_starting_x - pause_deletion_offset;
            }

            if starting_x + width > max_x {
                return None;
            }

            Some(Rect::from_min_size(
                Pos2::new(starting_x, starting_y),
                Vec2::new(width, 32.0),
            ))
        })
        .collect::<Option<Vec<_>>>()?;

    Some(match_rects)
}

fn calculate_match_card_rects(
    ui: &mut egui::Ui,
    match_card_min_width: f32,
    matches: &[SessionMatchDto],
) -> Vec<Rect> {
    let smallest_pause_ms = get_smallest_pause_ms(matches);
    let mut iteration = 0;

    loop {
        if let Some(rects) = calculate_match_card_rects_timeline(
            ui,
            iteration,
            match_card_min_width,
            matches,
            smallest_pause_ms,
        ) {
            ui.label(format!("Iterations: {}", iteration));
            break rects;
        }

        iteration += 1;

        if iteration > 10000 {
            ui.label("Iteration overflow");
            ui.label(format!("Iterations: {}", iteration));

            return vec![];
        }
    }
}
