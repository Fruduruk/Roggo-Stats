use eframe::egui::{self, Pos2, Rect, Vec2};

use crate::core::contract::session::SessionDto;

pub fn ui(ui: &mut egui::Ui, session: &SessionDto) -> egui::Response {
    let desired_size = egui::vec2(ui.available_width(), 80.0);
    let (timeline_rect, timeline_response) =
        ui.allocate_exact_size(desired_size, egui::Sense::click());
    let painter = ui.painter_at(timeline_rect);

    let (Some(first), Some(last)) = (session.matches.first(), session.matches.last()) else {
        return timeline_response;
    };

    let first_ms = first.created_at;
    let total_ms = (last.ended_at - first.created_at) as f32;

    let pixel_per_ms = timeline_rect.width() / total_ms;

    let match_rects = session
        .matches
        .iter()
        .map(|sm| {
            let width = (sm.ended_at - sm.created_at) as f32 * pixel_per_ms;

            let time_since_first = (sm.created_at  - first_ms) as f32;
            let starting_x = timeline_rect.left() + time_since_first * pixel_per_ms;

            Rect::from_min_size(
                Pos2::new(starting_x, timeline_rect.top()),
                Vec2::new(width, 32.0),
            )
        })
        .collect::<Vec<_>>();

    painter.line_segment(
        [
            egui::pos2(timeline_rect.left(), timeline_rect.center().y),
            egui::pos2(timeline_rect.right(), timeline_rect.center().y),
        ],
        egui::Stroke::new(2.0, egui::Color32::GRAY),
    );

    for rect in match_rects {
        let response = timeline_block(ui, rect, ui.id().with(format!("{}",rect.center().x)));
        if response.hovered() {
            // Hover
        }

        if response.clicked() {
            // Block geklickt
        }
    }

    timeline_response
}

fn timeline_block(ui: &mut egui::Ui, rect: Rect, id: egui::Id) -> egui::Response {
    let response = ui.interact(rect, id, egui::Sense::click());

    let color = if response.hovered() {
        egui::Color32::DARK_GRAY
    } else {
        egui::Color32::GRAY
    };

    ui.painter().rect_filled(rect, 5.0, color);

    ui.painter().text(
        rect.left_top() + egui::vec2(10.0, 10.0),
        egui::Align2::LEFT_TOP,
        "Match",
        egui::FontId::default(),
        egui::Color32::WHITE,
    );

    // let button_rect = Rect::from_min_size(
    //     rect.right_bottom() - egui::vec2(70.0, 35.0),
    //     egui::vec2(60.0, 25.0),
    // );

    // let button_response = ui.put(button_rect, egui::Button::new("Open"));

    // if button_response.clicked() {
    //     println!("Button clicked");
    // }

    response
}
