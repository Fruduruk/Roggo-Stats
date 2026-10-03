use eframe::egui::{self, Color32, Stroke, Vec2, pos2};

use crate::core::ui::theme::colors::colors;

#[derive(Default, Copy, Clone)]
pub struct DisplayValue {
    pub value: f32,
    pub color: Color32,
    pub emphasized: bool,
}

pub fn ui(ui: &mut egui::Ui, values: &[DisplayValue], size: Vec2, min_value: f32, max_value: f32) {
    if values.is_empty() {
        return;
    }

    let (rect, _) = ui.allocate_exact_size(size, egui::Sense::hover());
    let painter = ui.painter_at(rect);

    painter.rect_stroke(
        rect,
        3.0,
        egui::Stroke::new(1.0, colors(ui).border),
        egui::StrokeKind::Inside,
    );

    let center_x = rect.center().x;

    painter.line_segment(
        [pos2(center_x, rect.top()), pos2(center_x, rect.bottom())],
        Stroke::new(1.0, colors(ui).on_panel),
    );

    let zero_line_y = rect.bottom() - 5.0;

    painter.line_segment(
        [
            pos2(rect.left(), zero_line_y),
            pos2(rect.right(), zero_line_y),
        ],
        Stroke::new(1.0, colors(ui).text_weak),
    );

    let radius = 4.0;
    let spacing = radius * 2.0;
    let mut placed = Vec::<egui::Pos2>::new();

    for &display_value in values {
        let y = egui::remap(
            display_value.value,
            min_value..=max_value,
            zero_line_y..=rect.top(),
        );

        let mut step = 0;

        let pos = loop {
            let offset = match step {
                0 => 0.0,
                n if n % 2 == 1 => ((n + 1) / 2) as f32 * spacing,
                n => -(n / 2) as f32 * spacing,
            };

            let pos = pos2(center_x + offset, y);

            if placed.iter().all(|p| p.distance(pos) >= spacing) {
                break pos;
            }

            step += 1;
        };
        
        let rect = egui::Rect::from_center_size(pos, egui::vec2(radius * 2.0, radius * 2.0));

        let response = ui.interact(rect, ui.id().with(format!("{rect}")), egui::Sense::hover());

        painter.circle_filled(pos, radius, display_value.color);

        if display_value.emphasized {
            painter.circle(
                pos,
                radius + 3.0,
                display_value.color.gamma_multiply(1.2),
                Stroke::new(2.0, colors(ui).on_panel),
            );
        }

        response.on_hover_text_at_pointer(format!("{:.2}", display_value.value));
        placed.push(pos);
    }

    let average = values.iter().map(|dv| &dv.value).copied().sum::<f32>() / values.len() as f32;
    let average_y = egui::remap(average, min_value..=max_value, zero_line_y..=rect.top());

    painter.line_segment(
        [pos2(rect.left(), average_y), pos2(rect.right(), average_y)],
        Stroke::new(1.0, colors(ui).accent),
    );

    let accuracy = if max_value < 10.0 {
        2
    } else if max_value < 1000.0 {
        1
    } else {
        0
    };

    painter.text(
        pos2(rect.left() + 4.0, average_y - 2.0),
        egui::Align2::LEFT_BOTTOM,
        format!("{average:.accuracy$}"),
        egui::FontId::new(10.0, egui::FontFamily::Proportional),
        colors(ui).accent,
    );
}
