use eframe::egui;

use crate::core::ui::theme::colors::ThemeColors;

pub fn color_test(
    ui: &mut egui::Ui,
    colors: &ThemeColors,
) {
    ui.heading("Surfaces");

    ui.add_space(8.0);

    egui::Frame::new()
        .fill(colors.surface)
        .stroke(egui::Stroke::new(1.0, colors.border))
        .corner_radius(8.0)
        .inner_margin(16.0)
        .show(ui, |ui| {
            ui.heading("Surface");

            ui.label("Normal content");

            ui.label(
                egui::RichText::new("Weak content")
                    .color(colors.text_weak),
            );

            ui.add_space(8.0);

            ui.horizontal(|ui| {
                _ = ui.button("Button");

                ui.add_enabled(
                    false,
                    egui::Button::new("Disabled"),
                );
            });
        });

    ui.add_space(12.0);

    egui::Frame::new()
        .fill(colors.surface_alt)
        .stroke(egui::Stroke::new(1.0, colors.border))
        .corner_radius(8.0)
        .inner_margin(16.0)
        .show(ui, |ui| {
            ui.label("Alternative surface");
        });

    ui.add_space(24.0);

    ui.heading("Brand Colors");

    ui.add_space(8.0);

    color_block(
        ui,
        "Panel",
        colors.panel,
        colors.on_panel,
    );

    color_block(
        ui,
        "Accent",
        colors.accent,
        colors.on_accent,
    );

    ui.add_space(24.0);

    ui.heading("States");

    ui.label(
        egui::RichText::new("Success")
            .color(colors.success),
    );

    ui.label(
        egui::RichText::new("Warning")
            .color(colors.warning),
    );

    ui.label(
        egui::RichText::new("Error")
            .color(colors.error),
    );

    ui.add_space(24.0);

    ui.heading("egui Widgets");

    let mut input = "Test input".to_owned();

    ui.text_edit_singleline(&mut input);

    ui.horizontal(|ui| {
        _ = ui.selectable_label(false, "Inactive");
        _ = ui.selectable_label(true, "Selected");
    });

    _ = ui.button("Hover me");
}

fn color_block(
    ui: &mut egui::Ui,
    text: &str,
    background: egui::Color32,
    foreground: egui::Color32,
) {
    egui::Frame::new()
        .fill(background)
        .corner_radius(6.0)
        .inner_margin(12.0)
        .show(ui, |ui| {
            ui.label(
                egui::RichText::new(text)
                    .color(foreground),
            );
        });
}