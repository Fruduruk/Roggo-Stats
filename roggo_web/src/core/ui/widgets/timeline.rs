use std::collections::HashMap;

use eframe::egui::{self, Pos2, Rect, RichText, Vec2, pos2, vec2};

use crate::core::{
    contract::session::{SessionDto, SessionMatchDto},
    time::format_ms_time_without_seconds,
    ui::{components::tab_control::Tab, theme::colors::colors, widgets::match_card},
};

pub fn ui(
    ui: &mut egui::Ui,
    session: &SessionDto,
    selected_tab: &mut Tab,
) -> Option<egui::Response> {
    let spacing = 5.0;

    let (timeline_rects, timeline_boundary) = calculate_timeline_rects(ui, &session.matches)?;

    let (axis_rects, axis_boundary) =
        calculate_time_axis_rects(ui, &session.matches, timeline_boundary.bottom() + spacing)?;

    let (match_card_rects, match_cards_boundary) =
        calculate_match_card_rects(ui, &session.matches, axis_boundary.bottom() + spacing);

    let desired_size = timeline_boundary
        .union(axis_boundary)
        .union(match_cards_boundary)
        .size();
    let (full_rect, timeline_response) = ui.allocate_exact_size(desired_size, egui::Sense::click());

    let painter = ui.painter_at(full_rect);

    painter.rect_filled(
        Rect::from_min_size(
            pos2(ui.cursor().left(), axis_boundary.top()),
            vec2(ui.available_width(), 1.0),
        ),
        0.0,
        colors(ui).on_panel,
    );

    for (tick_rect, label_rect, label) in axis_rects {
        painter.rect_filled(tick_rect, 0.0, colors(ui).on_panel);
        painter.text(
            label_rect.center(),
            egui::Align2::CENTER_CENTER,
            label,
            egui::TextStyle::Small.resolve(ui.style()),
            colors(ui).on_panel,
        );
    }

    let responses = session
        .matches
        .iter()
        .zip(match_card_rects)
        .zip(timeline_rects)
        .map(|((m, mcr), tlr)| {
            (
                m.won,
                match_card::ui(ui, mcr, ui.id().with(m.match_guid), m, selected_tab),
                tlr,
            )
        })
        .collect::<Vec<_>>();

    let any_hovered = responses.iter().any(|(_, r, _)| r.hovered());

    for (won, response, rect) in responses {
        let original_color = match won {
            Some(true) => colors(ui).success,
            Some(false) => colors(ui).error,
            None => colors(ui).panel_shadow,
        }
        .gamma_multiply(0.9);

        let should_dim = any_hovered && !response.hovered();

        let animation = ui.ctx().animate_bool_with_time(
            ui.id().with(("timeline_hover", response.id)),
            should_dim,
            0.3,
        );

        let gamma = egui::lerp(1.0..=0.35, animation);

        let bonus = if response.hovered() { 0.34 } else { 0.0 };

        let color = original_color.gamma_multiply(gamma + bonus);

        painter.rect_filled(rect, 1.0, color);
    }
    Some(timeline_response)
}

fn test_outline(ui: &mut egui::Ui, rect: Rect) {
    ui.painter().rect_stroke(
        rect,
        1.0,
        egui::Stroke::new(1.0, colors(ui).on_panel),
        egui::StrokeKind::Inside,
    );
}

fn calculate_timeline_rects(
    ui: &egui::Ui,
    matches: &[SessionMatchDto],
) -> Option<(Vec<Rect>, Rect)> {
    let (Some(start), Some(end)) = (matches.first(), matches.last()) else {
        return None;
    };
    let total_ms = end.ended_at - start.created_at;
    let pixel_per_ms = ui.available_width() / total_ms as f32;
    let rects: Vec<_> = matches
        .iter()
        .map(|m| {
            let offset_ms = m.created_at - start.created_at;
            let duration_ms = m.ended_at - m.created_at;

            let x = ui.cursor().left() + offset_ms as f32 * pixel_per_ms;
            let width = duration_ms as f32 * pixel_per_ms;

            Rect::from_min_size(pos2(x, ui.cursor().top()), vec2(width, 7.0))
        })
        .collect();

    let boundary = rects.iter().copied().fold(Rect::NOTHING, Rect::union);

    Some((rects, boundary))
}

fn calculate_match_card_rects(
    ui: &egui::Ui,
    matches: &[SessionMatchDto],
    starting_y: f32,
) -> (Vec<Rect>, Rect) {
    let card_size = vec2(100.0, 32.0);
    let spacing = 5.0;

    let start_x = ui.cursor().left();
    let max_x = start_x + ui.available_width();

    let mut position = pos2(start_x, starting_y);

    let rects = matches
        .iter()
        .map(|_| {
            let exceeds_row = position.x + card_size.x > max_x;

            if exceeds_row && position.x > start_x {
                position.x = start_x;
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

fn calculate_time_axis_rects(
    ui: &egui::Ui,
    matches: &[SessionMatchDto],
    starting_y: f32,
) -> Option<(Vec<(Rect, Rect, String)>, Rect)> {
    let (Some(start), Some(end)) = (matches.first(), matches.last()) else {
        return None;
    };

    let start_ms = start.created_at;
    let end_ms = end.ended_at;
    let total_ms = end_ms - start_ms;

    let pixel_per_ms = ui.available_width() / total_ms as f32;
    let origin = pos2(ui.cursor().left(), starting_y);

    let quarter_hour_ms = 15 * 60 * 1000;

    let mut timestamp = ((start_ms + quarter_hour_ms - 1) / quarter_hour_ms) * quarter_hour_ms;

    let mut rects = vec![];

    let mut boundary = Rect::NOTHING;
    while timestamp <= end_ms {
        let offset_ms = timestamp - start_ms;
        let x = origin.x + offset_ms as f32 * pixel_per_ms;

        let tick_rect = Rect::from_min_size(pos2(x, origin.y), vec2(1.0, 5.0));

        let text = format_ms_time_without_seconds(timestamp);

        let galley = ui.painter().layout_no_wrap(
            text.clone(),
            egui::TextStyle::Small.resolve(ui.style()),
            colors(ui).on_panel,
        );

        let label_rect = Rect::from_center_size(
            pos2(x, tick_rect.bottom() + 2.0 + galley.size().y / 2.0),
            galley.size(),
        );

        boundary = boundary.union(tick_rect).union(label_rect);
        rects.push((tick_rect, label_rect, text));

        timestamp += quarter_hour_ms;
    }

    Some((rects, boundary))
}
