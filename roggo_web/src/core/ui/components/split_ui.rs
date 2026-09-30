use eframe::egui::{self, pos2, vec2, Rect, UiBuilder};

pub struct SplitUi;

impl SplitUi {
    pub fn show<R>(
        self,
        ui: &mut egui::Ui,
        ratio: f32,
        add_contents: impl FnOnce(&mut egui::Ui, &mut egui::Ui) -> R,
    ) -> R {
        let available_rect = ui.available_rect_before_wrap();
        let start_y = ui.cursor().top();

        let left_width = available_rect.width() / ratio;
        let right_width = available_rect.width() - left_width;

        let left_rect = Rect::from_min_size(
            available_rect.min,
            vec2(left_width, available_rect.height()),
        );

        let right_rect = Rect::from_min_size(
            pos2(left_rect.right(), available_rect.top()),
            vec2(right_width, available_rect.height()),
        );

        let layout = *ui.layout();

        let mut left_ui = ui.new_child(
            UiBuilder::new()
                .max_rect(left_rect)
                .layout(layout),
        );

        let mut right_ui = ui.new_child(
            UiBuilder::new()
                .max_rect(right_rect)
                .layout(layout),
        );

        let inner = add_contents(&mut left_ui, &mut right_ui);

        let bottom = left_ui
            .min_rect()
            .bottom()
            .max(right_ui.min_rect().bottom());

        let used_rect = Rect::from_min_max(
            pos2(available_rect.left(), start_y),
            pos2(available_rect.right(), bottom),
        );

        ui.advance_cursor_after_rect(used_rect);

        inner
    }
}