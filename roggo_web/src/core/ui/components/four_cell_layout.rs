use eframe::egui;

pub fn ui<S>(
    ui: &mut egui::Ui,
    min_left_width: f32,
    state: &mut S,
    mut top_left: impl FnMut(&mut S, &mut egui::Ui),
    mut top_right: impl FnMut(&mut S, &mut egui::Ui),
    mut bottom_left: impl FnMut(&mut S, &mut egui::Ui),
    mut bottom_right: impl FnMut(&mut S, &mut egui::Ui),
) {
    let full_rect = ui.available_rect_before_wrap();

    let top_left_size = ui
        .scope_builder(egui::UiBuilder::new().sizing_pass().invisible(), |ui| {
            top_left(state, ui);
        })
        .response
        .rect
        .size();

    let left_width = if top_left_size.x >= min_left_width {
        top_left_size.x
    } else {
        min_left_width
    };
    let top_height = top_left_size.y;

    let top_left_rect =
        egui::Rect::from_min_size(full_rect.min, egui::vec2(left_width, top_height));

    let top_right_rect = egui::Rect::from_min_max(
        egui::pos2(full_rect.left() + left_width, full_rect.top()),
        egui::pos2(full_rect.right(), full_rect.top() + top_height),
    );

    let bottom_left_rect = egui::Rect::from_min_max(
        egui::pos2(full_rect.left(), full_rect.top() + top_height),
        egui::pos2(full_rect.left() + left_width, full_rect.bottom()),
    );

    let bottom_right_rect = egui::Rect::from_min_max(
        egui::pos2(full_rect.left() + left_width, full_rect.top() + top_height),
        full_rect.max,
    );

    {
        let mut child = ui.new_child(
            egui::UiBuilder::new()
                .max_rect(top_left_rect)
                .id_salt("top_left"),
        );

        top_left(state, &mut child);
    }

    {
        let mut child = ui.new_child(
            egui::UiBuilder::new()
                .max_rect(top_right_rect)
                .id_salt("top_right"),
        );

        top_right(state, &mut child);
    }

    {
        let mut child = ui.new_child(
            egui::UiBuilder::new()
                .max_rect(bottom_left_rect)
                .id_salt("bottom_left"),
        );

        bottom_left(state, &mut child);
    }

    {
        let mut child = ui.new_child(
            egui::UiBuilder::new()
                .max_rect(bottom_right_rect)
                .id_salt("bottom_right"),
        );

        bottom_right(state, &mut child);
    }

    ui.advance_cursor_after_rect(full_rect);
}
