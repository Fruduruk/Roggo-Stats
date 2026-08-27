use eframe::egui;

pub fn tab_button(ui: &mut egui::Ui, text: String, selected: bool) -> egui::Response {
    let font_id = egui::TextStyle::Button.resolve(ui.style());

    let galley =
        ui.painter()
            .layout_no_wrap(text.clone(), font_id.clone(), ui.visuals().text_color());

    let padding_x = 16.0;
    let padding_y = 10.0;

    let size = egui::vec2(
        galley.size().x + padding_x * 2.0,
        galley.size().y + padding_y * 2.0,
    );

    let (rect, response) = ui.allocate_exact_size(size, egui::Sense::click());

    let text_color = if selected {
        ui.visuals().strong_text_color()
    } else if response.hovered() {
        ui.visuals().weak_text_color()
    } else {
        ui.visuals().text_color()
    };

    ui.painter().text(
        rect.center(),
        egui::Align2::CENTER_CENTER,
        text,
        font_id,
        text_color,
    );

    if selected {
        let underline_y = rect.bottom() - 2.0;

        ui.painter().line_segment(
            [
                egui::pos2(rect.left() + padding_x, underline_y),
                egui::pos2(rect.right() - padding_x, underline_y),
            ],
            egui::Stroke::new(2.0, ui.visuals().selection.bg_fill),
        );
    }

    response
}
