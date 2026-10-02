use crate::core::{
    ui::{theme::colors::colors},
};
use eframe::egui::{self, Image, ImageSource, Rect, Sense, vec2};

pub fn ui(
    ui: &mut egui::Ui,
    image: ImageSource,
    size: f32,
    hover_text: &str,
    enabled: bool,
) -> egui::Response {
    let sense = if enabled {
        Sense::click()
    } else {
        Sense::hover()
    };
    let (rect, response) = ui.allocate_exact_size(vec2(size, size), sense);

    let color = match (enabled, response.hovered()) {
        (false, _) => colors(ui).on_panel.gamma_multiply(0.25),
        (true, true) => colors(ui).on_panel.gamma_multiply(1.4),
        _ => colors(ui).on_panel,
    };

    let scale = if response.is_pointer_button_down_on() {
        0.85
    } else {
        1.0
    };

    let icon_size = vec2(size, size) * scale;

    Image::new(image)
        .fit_to_exact_size(icon_size)
        .tint(color)
        .paint_at(ui, Rect::from_center_size(rect.center(), icon_size));

    response.on_hover_text(hover_text)
}
