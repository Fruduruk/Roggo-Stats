use std::{
    sync::{Arc, Mutex},
    time::Duration,
};

use eframe::egui;

use crate::core::{
    contract::AgentErrorDto,
    ui::{match_overview_ui::MatchOverviewUi, tasks},
};

const COMPATIBLE_AGENT_VERSION: &str = "0.2.0";
const DOWNLOAD_URL: &'static str = "https://github.com/Fruduruk/Roggo-Stats/releases/download/roggo-agent-v0.2.0/RoggoAgentSetup_0.2.0.exe";

#[derive(Default)]
pub struct Content {
    pub player_name: Option<String>,
    pub current_error: Option<AgentErrorDto>,
    pub agent_version: Option<String>,
}

#[derive(Default)]
pub struct RoggoApp {
    last_reload: f64,
    match_overview_ui: MatchOverviewUi,
    content: Arc<Mutex<Content>>,
}

impl RoggoApp {
    pub fn new(cc: &eframe::CreationContext<'_>) -> Self {
        cc.egui_ctx.set_pixels_per_point(2.0);
        let app = Self::default();
        tasks::load_main_character(cc.egui_ctx.clone(), app.content.clone());
        app
    }
}

impl eframe::App for RoggoApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        ui.ctx().request_repaint_after(Duration::from_secs(1));

        let now = ui.ctx().input(|i| i.time);
        if self.last_reload + 1.0 < now {
            if let Ok(content) = self.content.lock() {
                if content.agent_version.is_none() {
                    tasks::load_version(ui.ctx().clone(), self.content.clone());
                }
                if content.player_name.is_none() {
                    tasks::load_main_character(ui.ctx().clone(), self.content.clone());
                }
                self.match_overview_ui.reload(ui.ctx().clone());
            }
            self.last_reload = now;
        }

        egui::Panel::top("header").show_inside(ui, |ui| {
            ui.horizontal(|ui| {
                ui.heading("Roggo Stats Monitor");

                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    if let Ok(content) = self.content.lock() {
                        if let Some(name) = &content.player_name {
                            ui.label(name);
                        }
                    }

                    // egui::widgets::global_theme_preference_switch(ui);
                });
            })
        });

        egui::CentralPanel::default().show_inside(ui, |ui| {
            if let Ok(content) = self.content.lock() {
                if let Some(player_name) = &content.player_name {
                    self.match_overview_ui.ui(ui, player_name);
                }
            }
            ui.with_layout(egui::Layout::top_down(egui::Align::Center), |ui| {
                if let Ok(content) = self.content.lock() {
                    if let Some(error) = &content.current_error {
                        // web_sys::console::log_1(&"Hallo aus WASM".into());
                        ui.label(&error.message);
                    }
                }
            });
        });

        if let Ok(content) = self.content.lock() {
            let version = content.agent_version.as_deref().unwrap_or("0".into());

            if version != COMPATIBLE_AGENT_VERSION {
                egui::Area::new("center_message".into())
                    .anchor(egui::Align2::CENTER_CENTER, egui::Vec2::ZERO)
                    .order(egui::Order::Foreground)
                    .show(ui.ctx(), |ui| {
                        egui::Frame::popup(ui.style()).show(ui, |ui| {
                            ui.label(format!(
                                "Please download and run roggo agent with version {}",
                                COMPATIBLE_AGENT_VERSION
                            ));

                            ui.hyperlink_to("download here", DOWNLOAD_URL);
                        });
                    });
            }
        }
    }
}
