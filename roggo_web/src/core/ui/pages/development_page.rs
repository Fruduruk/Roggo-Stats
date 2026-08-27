use eframe::egui;

use crate::core::ui::{
    components::tab_control::{Tab, TabControl},
    theme::{
        colors::{colors, reset_colors, set_colors},
    },
    widgets::{color_test::color_test, live_editor::theme_editor},
};

#[derive(Default)]
pub struct DevelopmentPage {
    tab_control: TabControl,
}

impl DevelopmentPage {
    pub fn ui(&mut self, ui: &mut egui::Ui) {
        egui::CentralPanel::default().show(ui, |ui| {
            let tab = self.tab_control.ui(ui);

            ui.label(tab.to_string());
            if tab == Tab::AllTime {
                theme_edit(ui);
            }
        });
    }
}

fn theme_edit(ui: &mut egui::Ui) {
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
