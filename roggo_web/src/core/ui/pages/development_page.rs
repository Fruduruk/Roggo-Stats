use eframe::egui;

use crate::core::{
    app_state::AppState,
    ui::{
        theme::colors::{colors, reset_colors, set_colors},
        widgets::{color_test::color_test, live_editor::theme_editor},
    },
};

#[derive(Default)]
pub struct DevelopmentPage {
}

impl DevelopmentPage {
    pub fn ui(&mut self, ui: &mut egui::Ui, state: &mut AppState) {
        
    }
}

pub fn theme_edit(ui: &mut egui::Ui) {
    let current_theme = ui.theme();

    let mut colors = colors(ui);
    let ctx = ui.ctx().clone();
    egui::ScrollArea::vertical().show(ui, |ui| {
        ui.columns(2, |columns| {
            color_test(&mut columns[0], &colors);

            let changed = theme_editor(&mut columns[1], &mut colors);

            if changed {
                set_colors(&ctx, current_theme, colors);
            }
        });
        if ui.button("Reset current theme").clicked() {
            reset_colors(ui.ctx(), current_theme);
        }
    });
}
