use crate::core::{app::UI_VERSION, ui::theme::colors::colors};
use eframe::egui;

const GITHUB_URL: &str = "https://github.com/Fruduruk/Roggo-Stats";

pub fn ui(ui: &mut egui::Ui, agent_version: Option<String>) {
    egui::Panel::bottom("footer")
        .frame(
            egui::Frame::new()
                .fill(colors(ui).background)
                .inner_margin(5.0),
        )
        .show_separator_line(false)
        .show(ui, |ui| {
            ui.horizontal(|ui| {
                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    show_github_button(ui);
                    egui::widgets::global_theme_preference_switch(ui);
                    if agent_version.is_some() {
                        ui.label(UI_VERSION.to_string());
                    }
                });
            });
        });
}

fn show_github_button(ui: &mut egui::Ui) {
    let image_source = match ui.theme() {
        egui::Theme::Dark => egui::include_image!("../../../../assets/github_dark.png"),
        egui::Theme::Light => egui::include_image!("../../../../assets/github_light.png"),
    };

    let image = egui::Image::new(image_source).fit_to_exact_size(egui::vec2(16.0, 16.0));

    let response = ui
        .add(
            egui::Button::image(image)
                .min_size(egui::vec2(22.0, 22.0))
                .corner_radius(egui::CornerRadius::same(12))
                .frame(false)
                .frame_when_inactive(false),
        )
        .on_hover_text(GITHUB_URL);

    if response.clicked() {
        ui.open_url(egui::OpenUrl {
            url: GITHUB_URL.into(),
            new_tab: true,
        });
    }
}
