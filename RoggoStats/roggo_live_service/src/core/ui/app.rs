use std::{
    sync::{Arc, Mutex},
    time::Duration,
};

use eframe::egui;

use crate::core::{
    dto::AgentErrorDto,
    ui::{match_overview_ui::MatchOverviewUi, tasks},
};

#[derive(Default)]
pub struct Content {
    pub player_name: Option<String>,
    pub current_error: Option<AgentErrorDto>,
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
        tasks::load_main_character(app.content.clone());
        app
    }
}

impl eframe::App for RoggoApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        ui.ctx().request_repaint_after(Duration::from_secs(1));

        let now = ui.ctx().input(|i| i.time);
        if self.last_reload + 1.0 < now {
            if let Ok(content) = self.content.lock() {
                if content.player_name.is_none() {
                    tasks::load_main_character(self.content.clone());
                }
                self.match_overview_ui.reload();
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
            self.match_overview_ui.update(ui);

            ui.with_layout(egui::Layout::top_down(egui::Align::Center), |ui| {
                if let Ok(content) = self.content.lock() {
                    if let Some(error) = &content.current_error {
                        // web_sys::console::log_1(&"Hallo aus WASM".into());
                        ui.label(&error.message);
                    }
                }
            });
        });
    }
}
