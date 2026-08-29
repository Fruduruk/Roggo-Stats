use eframe::egui;

pub fn tab_button(
    ui: &mut egui::Ui,
    text: String,
    selected: bool,
) -> egui::Response {
    let font_id = egui::TextStyle::Button.resolve(ui.style());

    let galley = ui.painter().layout_no_wrap(
        text.clone(),
        font_id.clone(),
        ui.visuals().text_color(),
    );

    let (rect, response) =
        ui.allocate_exact_size(galley.size(), egui::Sense::click());

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

    response
}