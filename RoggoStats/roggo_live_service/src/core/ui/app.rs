use std::{
    sync::{Arc, Mutex},
    time::Duration,
};

use eframe::egui;

use crate::core::{
    contract::AgentErrorDto,
    ui::{install_ui::InstallUi, match_overview_ui::MatchOverviewUi, tasks},
};
pub const UI_VERSION: &str = "0.5.0";
pub const COMPATIBLE_AGENT_VERSION: &str = "0.5.0";

#[derive(Default)]
pub struct Content {
    pub player_name: Option<String>,
    pub current_error: Option<AgentErrorDto>,
    pub agent_version: Option<String>,
}

pub struct RoggoApp {
    last_reload: f64,
    match_overview_ui: MatchOverviewUi,
    install_ui: InstallUi,
    content: Arc<Mutex<Content>>,
    full_reload_requested: Arc<Mutex<bool>>,
}

impl RoggoApp {
    pub fn new(cc: &eframe::CreationContext<'_>) -> Self {
        cc.egui_ctx.set_pixels_per_point(2.0);

        let app = RoggoApp::new_with_single_reload_arc();

        tasks::load_main_character(cc.egui_ctx.clone(), app.content.clone());
        app
    }

    fn get_agent_version(&self) -> String {
        if let Ok(content) = self.content.lock() {
            content
                .agent_version
                .as_deref()
                .unwrap_or(&String::new())
                .into()
        } else {
            String::new()
        }
    }

    pub fn new_with_single_reload_arc() -> Self {
        let arc: Arc<Mutex<bool>> = Default::default();
        Self {
            last_reload: 0.0,
            match_overview_ui: MatchOverviewUi::new_with_single_reload_arc(arc.clone()),
            content: Default::default(),
            full_reload_requested: arc,
            install_ui: Default::default(),
        }
    }

    fn reload_cycle(&mut self, ui: &mut egui::Ui) {
        ui.ctx().request_repaint_after(Duration::from_secs(1));

        let now = ui.ctx().input(|i| i.time);
        if let Ok(mut full_reload_requested) = self.full_reload_requested.lock() {
            if self.last_reload + 1.0 < now || *full_reload_requested {
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
                *full_reload_requested = false;
            }
        }
    }

    fn main_ui(&mut self, ui: &mut egui::Ui) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            if let Ok(content) = self.content.lock() {
                if let Some(player_name) = &content.player_name {
                    self.match_overview_ui.ui(ui, player_name);
                }
            }
            ui.with_layout(egui::Layout::top_down(egui::Align::Center), |ui| {
                if let Ok(content) = self.content.lock() {
                    if let Some(error) = &content.current_error {
                        ui.label(&error.message);
                    }
                }
            });
        });
    }
}

impl eframe::App for RoggoApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        self.reload_cycle(ui);

        let version = self.get_agent_version();

        egui::Panel::top("header").show_inside(ui, |ui| {
            ui.horizontal(|ui| {
                ui.heading("Roggo Stats");

                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    if let Ok(content) = self.content.lock() {
                        if let Some(name) = &content.player_name {
                            ui.label(name);
                        }
                    }
                    ui.separator();
                    // egui::widgets::global_theme_preference_switch(ui);
                    if !version.is_empty() {
                        ui.label(&format!("WebUi version {UI_VERSION}"));
                    }
                });
            })
        });

        if version == COMPATIBLE_AGENT_VERSION {
            self.main_ui(ui);
        } else {
            self.install_ui.ui(ui, version);
        }
    }
}
