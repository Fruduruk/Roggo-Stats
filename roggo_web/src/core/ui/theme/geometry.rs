use eframe::egui;

pub fn apply_size_spacing_corners(ctx: &egui::Context) {
    ctx.all_styles_mut(|style| {
        style.text_styles.insert(
            egui::TextStyle::Heading,
            egui::FontId::new(23.0, egui::FontFamily::Proportional),
        );

        style.text_styles.insert(
            egui::TextStyle::Body,
            egui::FontId::new(15.0, egui::FontFamily::Proportional),
        );

        style.text_styles.insert(
            egui::TextStyle::Small,
            egui::FontId::new(13.0, egui::FontFamily::Proportional),
        );

        style.spacing.item_spacing = egui::vec2(6.0, 6.0);

        style.spacing.button_padding = egui::vec2(10.0, 6.0);

        let radius = egui::CornerRadius::same(6);

        style.visuals.widgets.noninteractive.corner_radius = radius;
        style.visuals.widgets.inactive.corner_radius = radius;
        style.visuals.widgets.hovered.corner_radius = radius;
        style.visuals.widgets.active.corner_radius = radius;
        style.visuals.widgets.open.corner_radius = radius;

        style.visuals.window_corner_radius = egui::CornerRadius::same(8);

        style.visuals.menu_corner_radius = egui::CornerRadius::same(6);
    });
}

