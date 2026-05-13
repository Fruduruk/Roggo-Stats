use std::{sync::{Arc, Mutex}, time::Duration};

use eframe::egui;

use crate::core::ui::{match_overview_ui::MatchOverviewUi, tasks};

#[derive(Default)]
pub struct Content {
    pub player_name: Option<String>,
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
        tasks::load_player_name(app.content.clone(), cc.egui_ctx.clone());
        app
    }
}

impl eframe::App for RoggoApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        ui.ctx().request_repaint_after(Duration::from_secs(2));

        let now = ui.ctx().input(|i| i.time);
        if self.last_reload + 1.0 < now {
            tasks::load_player_name(self.content.clone(), ui.ctx().clone());
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

                    egui::widgets::global_theme_preference_switch(ui);
                });
            })
        });

        egui::CentralPanel::default().show_inside(ui, |ui| {
            self.match_overview_ui.update(ui);
        });
    }
}
