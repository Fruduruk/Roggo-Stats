use eframe::egui::{self, Rect, UiBuilder, pos2, vec2};

pub struct SplitUi;

impl SplitUi {
    pub fn show<R>(
        self,
        ui: &mut egui::Ui,
        ratio: f32,
        min_width_left: f32,
        min_width_right: f32,
        add_contents: impl FnOnce(&mut egui::Ui, &mut egui::Ui) -> R,
    ) -> Option<R> {
        let available_rect = ui.available_rect_before_wrap();
        let start_y = ui.cursor().top();

        let width = available_rect.width();

        if min_width_left + min_width_right > width {
            return None;
        }

        let min_ratio = width / (width - min_width_right);
        let max_ratio = width / min_width_left;

        let ratio = ratio.clamp(min_ratio, max_ratio);

        let left_width = width / ratio;
        let right_width = width - left_width;


        let left_rect = Rect::from_min_size(
            available_rect.min,
            vec2(left_width, available_rect.height()),
        );

        let right_rect = Rect::from_min_size(
            pos2(left_rect.right(), available_rect.top()),
            vec2(right_width, available_rect.height()),
        );

        let layout = *ui.layout();

        let mut left_ui = ui.new_child(UiBuilder::new().max_rect(left_rect).layout(layout));

        let mut right_ui = ui.new_child(UiBuilder::new().max_rect(right_rect).layout(layout));

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

        Some(inner)
    }
}
