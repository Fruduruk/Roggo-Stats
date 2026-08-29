use eframe::egui;

use crate::core::ui::theme::colors::colors;

pub struct FullPanel;

impl FullPanel {
    pub fn show<R>(
        self,
        ui: &mut egui::Ui,
        add_contents: impl FnOnce(&mut egui::Ui) -> R,
    ) -> egui::InnerResponse<R> {
        egui::Frame::new()
            .corner_radius(5.0)
            .outer_margin(egui::Margin::symmetric(8, 4))
            .inner_margin(10.0)
            .fill(colors(ui).panel)
            .shadow(egui::Shadow {
                offset: [2, 2],
                blur: 2,
                spread: 1,
                color: colors(ui).panel_shadow,
            })
            .show(ui, |ui| {
                ui.set_min_size(ui.available_size());
                add_contents(ui)
            })
    }
}
