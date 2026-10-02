use crate::core::{
    Error,
    app::UI_VERSION,
    icons,
    ui::{theme::colors::colors, widgets::icon_button},
};
use eframe::egui;
use roggo_contract::PlayerDto;

const GITHUB_URL: &str = "https://github.com/Fruduruk/Roggo-Stats";

pub fn ui(
    ui: &mut egui::Ui,
    agent_version: Option<String>,
    general_errors: &Vec<Error>,
    player_name: &Option<PlayerDto>,
) {
    egui::Panel::bottom("footer")
        .frame(
            egui::Frame::new()
                .fill(colors(ui).background)
                .inner_margin(egui::Margin::symmetric(10, 4)),
        )
        .show_separator_line(false)
        .show(ui, |ui| {
            ui.horizontal(|ui| {
                if let Some(dto) = player_name {
                    ui.label(&dto.display_name);
                }

                ui.label(" ")
                    .on_hover_text(format!("{:#?}", general_errors));
                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    if icon_button::ui(ui, icons::GITHUB_DARK, 18.0, GITHUB_URL, true).clicked() {
                        ui.open_url(egui::OpenUrl {
                            url: GITHUB_URL.into(),
                            new_tab: true,
                        });
                    }
                    if agent_version.is_some() {
                        ui.label(UI_VERSION.to_string());
                    }
                    egui::widgets::global_theme_preference_switch(ui);
                });
            });
        });
}
