use eframe::egui;

use crate::core::ui::theme::colors::ThemeColors;

pub fn theme_editor(ui: &mut egui::Ui, colors: &mut ThemeColors) -> bool {
    let mut changed = false;

    ui.heading("Theme Editor");

    ui.add_space(12.0);

    egui::Grid::new("theme_editor")
        .num_columns(2)
        .spacing([16.0, 8.0])
        .show(ui, |ui| {
            changed |= color_edit_row(ui, "Background", &mut colors.background);

            changed |= color_edit_row(ui, "Surface", &mut colors.surface);

            changed |= color_edit_row(ui, "Surface Alt", &mut colors.surface_alt);

            changed |= color_edit_row(ui, "Surface Hover", &mut colors.surface_hover);

            changed |= color_edit_row(ui, "Surface Shadow", &mut colors.surface_shadow);

            changed |= color_edit_row(ui, "Primary", &mut colors.primary);

            changed |= color_edit_row(ui, "Primary Hover", &mut colors.primary_hover);

            changed |= color_edit_row(ui, "On Primary", &mut colors.on_primary);

            changed |= color_edit_row(ui, "Secondary", &mut colors.secondary);

            changed |= color_edit_row(ui, "Secondary Hover", &mut colors.secondary_hover);

            changed |= color_edit_row(ui, "On Secondary", &mut colors.on_secondary);

            changed |= color_edit_row(ui, "Accent", &mut colors.accent);

            changed |= color_edit_row(ui, "Accent Hover", &mut colors.accent_hover);

            changed |= color_edit_row(ui, "On Accent", &mut colors.on_accent);

            changed |= color_edit_row(ui, "Text", &mut colors.text);

            changed |= color_edit_row(ui, "Text Weak", &mut colors.text_weak);

            changed |= color_edit_row(ui, "Border", &mut colors.border);

            changed |= color_edit_row(ui, "Success", &mut colors.success);

            changed |= color_edit_row(ui, "Warning", &mut colors.warning);

            changed |= color_edit_row(ui, "Error", &mut colors.error);
        });

    changed
}

fn color_edit_row(ui: &mut egui::Ui, label: &str, color: &mut egui::Color32) -> bool {
    ui.label(label);

    let changed = ui.color_edit_button_srgba(color).changed();

    ui.end_row();

    changed
}
