use eframe::egui::{self, Rect, Response, pos2, vec2};
use uuid::Uuid;

use crate::core::{
    contract::session::{SessionDto, SessionMatchDto},
    time::format_ms_time_without_seconds,
    ui::theme::colors::colors,
};

pub fn ui(
    ui: &mut egui::Ui,
    session: &SessionDto,
    hovered_match_guid: &Option<Uuid>,
) -> Option<Response> {
    let spacing = 5.0;

    let (timeline_rects, timeline_boundary) = calculate_timeline_rects(ui, &session.matches)?;

    let (axis_rects, axis_boundary) =
        calculate_time_axis_rects(ui, &session.matches, timeline_boundary.bottom() + spacing)?;

    let desired_size = timeline_boundary.union(axis_boundary).size();
    let (full_rect, response) = ui.allocate_exact_size(desired_size, egui::Sense::click());

    let painter = ui.painter_at(full_rect);

    paint_timeline_axis(ui, axis_rects, axis_boundary, &painter);

    let any_hovered = hovered_match_guid.is_some();

    for (m, rect) in session.matches.iter().zip(timeline_rects) {
        let match_hovered = hovered_match_guid == &Some(m.match_guid);
        let should_dim = any_hovered && !match_hovered;

        let original_color = match m.won {
            Some(true) => colors(ui).success,
            Some(false) => colors(ui).error,
            None => colors(ui).panel_shadow,
        }
        .gamma_multiply(0.9);

        let animation = ui.ctx().animate_bool_with_time(
            ui.id().with(("timeline_hover", m.match_guid)),
            should_dim,
            0.3,
        );

        let gamma = egui::lerp(1.0..=0.35, animation);
        let hover_bonus = if match_hovered { 0.34 } else { 0.0 };

        painter.rect_filled(
            rect,
            1.0,
            original_color.gamma_multiply(gamma + hover_bonus),
        );
    }
    Some(response)
}

fn paint_timeline_axis(
    ui: &mut egui::Ui,
    axis_rects: Vec<(Rect, Rect, String)>,
    axis_boundary: Rect,
    painter: &egui::Painter,
) {
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

    let start_x = ui.cursor().left();
    let tick_height = 5.0;
    let label_spacing = 2.0;

    let quarter_hour_ms = 15 * 60 * 1000;

    let mut timestamp = ((start_ms + quarter_hour_ms - 1) / quarter_hour_ms) * quarter_hour_ms;

    let mut rects = vec![];

    while timestamp <= end_ms {
        let offset_ms = timestamp - start_ms;
        let x = start_x + offset_ms as f32 * pixel_per_ms;

        let tick_rect = Rect::from_min_size(pos2(x, starting_y), vec2(1.0, tick_height));

        let text = format_ms_time_without_seconds(timestamp);

        let galley = ui.painter().layout_no_wrap(
            text.clone(),
            egui::TextStyle::Small.resolve(ui.style()),
            colors(ui).on_panel,
        );

        let label_rect = Rect::from_center_size(
            pos2(
                x,
                tick_rect.bottom() + label_spacing + galley.size().y / 2.0,
            ),
            galley.size(),
        );

        rects.push((tick_rect, label_rect, text));

        timestamp += quarter_hour_ms;
    }

    let label_height = egui::TextStyle::Small.resolve(ui.style()).size;

    let boundary = Rect::from_min_size(
        pos2(start_x, starting_y),
        vec2(
            ui.available_width(),
            tick_height + label_spacing + label_height,
        ),
    );

    Some((rects, boundary))
}
