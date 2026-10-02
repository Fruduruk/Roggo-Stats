use eframe::egui::{self, Stroke, Vec2, pos2};

use crate::core::ui::theme::colors::colors;

pub fn ui(ui: &mut egui::Ui, values: &[f32], size: Vec2, min_value: f32, max_value: f32) {
    let (rect, _) = ui.allocate_exact_size(size, egui::Sense::hover());
    let painter = ui.painter_at(rect);

    painter.rect_stroke(
        rect,
        3.0,
        egui::Stroke::new(1.0, colors(ui).border),
        egui::StrokeKind::Inside,
    );

    let x = rect.center().x;

    painter.line_segment(
        [pos2(x, rect.top()), pos2(x, rect.bottom())],
        Stroke::new(1.0, colors(ui).on_panel),
    );

    let radius = 4.0;
    let spacing = radius * 2.0;
    let mut placed = Vec::<egui::Pos2>::new();

    for &value in values {
        let y = egui::remap(value, min_value..=max_value, rect.bottom()..=rect.top());

        let mut step = 0;

        let pos = loop {
            let offset = match step {
                0 => 0.0,
                n if n % 2 == 1 => ((n + 1) / 2) as f32 * spacing,
                n => -(n / 2) as f32 * spacing,
            };

            let pos = pos2(x + offset, y);

            if placed.iter().all(|p| p.distance(pos) >= spacing) {
                break pos;
            }

            step += 1;
        };

        painter.circle_filled(pos, radius, ui.visuals().text_color());
        placed.push(pos);
    }
}
