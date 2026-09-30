use eframe::egui::{self, Rect, Response, vec2};
use uuid::Uuid;

use crate::core::{
    contract::session::{SessionDto, SessionMatchDto},
    ui::{ widgets::match_card},
};

pub fn ui(ui: &mut egui::Ui, session: &SessionDto) -> Option<(Response, Uuid)> {
    let (rects, bounding) = calculate_match_card_rects(ui, &session.matches);
    let responses = session
        .matches
        .iter()
        .zip(rects)
        .map(|(m, mcr)| (m, match_card::ui(ui, mcr, ui.id().with(m.match_guid), m)))
        .collect::<Vec<_>>();

    let (full_rect, _) = ui.allocate_exact_size(bounding.size(), egui::Sense::click());

    if let Some((interacted_session_match_dto, response)) = responses
        .into_iter()
        .filter(|(_, r)| r.clicked() || r.hovered())
        .collect::<Vec<_>>()
        .first()
    {
        return Some((response.clone(), interacted_session_match_dto.match_guid));
    }

    None
}

fn calculate_match_card_rects(ui: &egui::Ui, matches: &[SessionMatchDto]) -> (Vec<Rect>, Rect) {
    let card_size = vec2(80.0, 50.0);
    let spacing = 5.0;

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
