use eframe::egui::{self, Color32, Painter, Pos2, Rect, Response, TextStyle, pos2, vec2};
use uuid::Uuid;

use crate::core::{
    contract::session::{SessionDto, SessionMatchDto},
    time::format_ms_time_without_seconds,
    ui::theme::colors::colors,
};

const TIMELINE_HEIGHT: f32 = 7.0;
const TIMELINE_AXIS_SPACING: f32 = 5.0;

const AXIS_LINE_HEIGHT: f32 = 1.0;
const AXIS_TICK_HEIGHT: f32 = 5.0;
const AXIS_LABEL_SPACING: f32 = 2.0;

const FIVE_MIN_MS: i64 = 5 * 60 * 1000;
const QUARTER_HOUR_MS: i64 = 15 * 60 * 1000;
const HOUR_MS: i64 = 4 * QUARTER_HOUR_MS;
const HALF_HOUR_MS: i64 = 2 * QUARTER_HOUR_MS;

struct TimelineLayout {
    match_rects: Vec<Rect>,
    axis: AxisLayout,
    boundary: Rect,
}

struct AxisLayout {
    boundary: Rect,
    line_rect: Rect,
    ticks: Vec<AxisTick>,
}

struct AxisTick {
    tick_rect: Rect,
    label_rect: Rect,
    label: String,
}

pub fn ui(
    ui: &mut egui::Ui,
    session: &SessionDto,
    hovered_match_guid: &Option<Uuid>,
) -> Option<Response> {
    let layout = calculate_layout(ui, &session.matches)?;

    let (_, response) = ui.allocate_exact_size(layout.boundary.size(), egui::Sense::click());

    let painter = ui.painter();

    paint_axis(ui, painter, &layout.axis);

    paint_matches(
        ui,
        painter,
        &session.matches,
        &layout.match_rects,
        *hovered_match_guid,
    );

    Some(response)
}

fn calculate_layout(ui: &egui::Ui, matches: &[SessionMatchDto]) -> Option<TimelineLayout> {
    let start = matches.first()?;
    let end = matches.last()?;

    let origin = ui.cursor().left_top();
    let width = ui.available_width();

    let start_ms = start.created_at;
    let end_ms = end.ended_at;

    let total_ms = end_ms - start_ms;

    if total_ms <= 0 {
        return None;
    }

    let pixel_per_ms = width / total_ms as f32;

    let match_rects = calculate_match_rects(matches, origin, start_ms, pixel_per_ms);

    let timeline_boundary = Rect::from_min_size(origin, vec2(width, TIMELINE_HEIGHT));

    let axis_origin_y = timeline_boundary.bottom() + TIMELINE_AXIS_SPACING;

    let interval = match matches.len() {
        0..=10 => FIVE_MIN_MS,
        11..=20 => QUARTER_HOUR_MS,
        21..=30 => HALF_HOUR_MS,
        _ => HOUR_MS
    };

    let axis = calculate_axis_layout(
        ui,
        start_ms,
        end_ms,
        pos2(origin.x, axis_origin_y),
        width,
        pixel_per_ms,
        interval
    );

    let boundary = timeline_boundary.union(axis.boundary);

    Some(TimelineLayout {
        match_rects,
        axis,
        boundary,
    })
}

fn calculate_match_rects(
    matches: &[SessionMatchDto],
    origin: egui::Pos2,
    start_ms: i64,
    pixel_per_ms: f32,
) -> Vec<Rect> {
    matches
        .iter()
        .map(|m| {
            let offset_ms = m.created_at - start_ms;
            let duration_ms = m.ended_at - m.created_at;

            let x = origin.x + offset_ms as f32 * pixel_per_ms;

            let width = duration_ms as f32 * pixel_per_ms;

            Rect::from_min_size(pos2(x, origin.y), vec2(width, TIMELINE_HEIGHT))
        })
        .collect()
}

fn calculate_axis_layout(
    ui: &egui::Ui,
    start_ms: i64,
    end_ms: i64,
    start: Pos2,
    width: f32,
    pixel_per_ms: f32,
    interval: i64,
) -> AxisLayout {
    let font_id = TextStyle::Small.resolve(ui.style());

    let label_height = font_id.size;

    let axis_height = AXIS_TICK_HEIGHT + AXIS_LABEL_SPACING + label_height;

    let boundary = Rect::from_min_size(start, vec2(width, axis_height));

    let line_rect = Rect::from_min_size(start, vec2(width, AXIS_LINE_HEIGHT));

    let mut timestamp = ceil_to_interval(start_ms, interval);

    let mut ticks = vec![];

    while timestamp <= end_ms {
        let offset_ms = timestamp - start_ms;

        let x = start.x + offset_ms as f32 * pixel_per_ms;

        let tick_rect =
            Rect::from_min_size(pos2(x, start.y), vec2(AXIS_LINE_HEIGHT, AXIS_TICK_HEIGHT));

        let label = format_ms_time_without_seconds(timestamp);

        let galley =
            ui.painter()
                .layout_no_wrap(label.clone(), font_id.clone(), colors(ui).on_panel);

        let label_rect = Rect::from_center_size(
            pos2(
                x,
                tick_rect.bottom() + AXIS_LABEL_SPACING + galley.size().y / 2.0,
            ),
            galley.size(),
        );

        ticks.push(AxisTick {
            tick_rect,
            label_rect,
            label,
        });

        timestamp += interval;
    }

    AxisLayout {
        boundary,
        line_rect,
        ticks,
    }
}

fn paint_axis(ui: &egui::Ui, painter: &Painter, axis: &AxisLayout) {
    let color = colors(ui).on_panel;
    let font_id = TextStyle::Small.resolve(ui.style());

    painter.rect_filled(axis.line_rect, 0.0, color);

    for tick in &axis.ticks {
        painter.rect_filled(tick.tick_rect, 0.0, color);

        painter.text(
            tick.label_rect.center(),
            egui::Align2::CENTER_CENTER,
            &tick.label,
            font_id.clone(),
            color,
        );
    }
}

fn paint_matches(
    ui: &egui::Ui,
    painter: &Painter,
    matches: &[SessionMatchDto],
    rects: &[Rect],
    hovered_match_guid: Option<Uuid>,
) {
    let any_hovered = hovered_match_guid.is_some();

    for (m, rect) in matches.iter().zip(rects) {
        let match_hovered = hovered_match_guid == Some(m.match_guid);

        let should_dim = any_hovered && !match_hovered;

        let base_color = match_color(ui, m).gamma_multiply(0.9);

        let animation = ui.ctx().animate_bool_with_time(
            ui.id().with(("timeline_hover", m.match_guid)),
            should_dim,
            0.3,
        );

        let gamma = egui::lerp(1.0..=0.35, animation);

        let hover_bonus = if match_hovered { 0.34 } else { 0.0 };

        let color = base_color.gamma_multiply(gamma + hover_bonus);

        painter.rect_filled(*rect, 1.0, color);
    }
}

fn match_color(ui: &egui::Ui, m: &SessionMatchDto) -> Color32 {
    match m.won {
        Some(true) => colors(ui).success,
        Some(false) => colors(ui).error,
        None => colors(ui).panel_shadow,
    }
}

fn ceil_to_interval(value: i64, interval: i64) -> i64 {
    ((value + interval - 1) / interval) * interval
}
