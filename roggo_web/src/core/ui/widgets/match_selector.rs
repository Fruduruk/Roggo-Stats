use crate::core::ui::widgets::match_card;
use eframe::egui::{self, Rect, Response, vec2};
use roggo_contract::*;
use uuid::Uuid;

pub fn ui(
    ui: &mut egui::Ui,
    session: &SessionDto,
    selected_match_guid: &Option<Uuid>,
) -> Option<(Response, Uuid)> {
    let (rects, bounding) = calculate_match_card_rects(ui, &session.matches);
    let (_, _) = ui.allocate_exact_size(bounding.size(), egui::Sense::click());

    let responses = session
        .matches
        .iter()
        .zip(rects)
        .map(|(m, mcr)| {
            (
                match_card::ui(ui, mcr, ui.id().with(m.match_guid), m, selected_match_guid),
                m.match_guid,
            )
        })
        .collect::<Vec<_>>();

    responses
        .into_iter()
        .find(|(r, _)| r.clicked() || r.hovered())
}

fn calculate_match_card_rects(ui: &egui::Ui, matches: &[SessionMatchDto]) -> (Vec<Rect>, Rect) {
    let card_size = vec2(ui.available_width(), 20.0);
    let spacing = 3.0;

    let start = ui.cursor().left_top();
    let max_x = start.x + ui.available_width();

    let mut position = start;

    let rects = matches
        .iter()
        .map(|_| {
            let exceeds_row = position.x + card_size.x > max_x;

            if exceeds_row && position.x > start.x {
                position.x = start.x;
                position.y += card_size.y + spacing;
            }

            let rect = Rect::from_min_size(position, card_size);

            position.x += card_size.x + spacing;

            rect
        })
        .collect::<Vec<_>>();

    let boundary = rects.iter().copied().fold(Rect::NOTHING, Rect::union);

    (rects, boundary)
}
