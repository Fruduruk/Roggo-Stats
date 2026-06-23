#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::fs;

use eframe::egui;
use roggo_agent::{AgentConfig, get_config_file_path, load_config_or_default};
const UNSAVED_NOTIFICATION: &str = "Not saved yet.";

struct ConfigApp {
    config: AgentConfig,
    status: String,
}

impl ConfigApp {
    fn new(cc: &eframe::CreationContext) -> Self {
        cc.egui_ctx.set_pixels_per_point(1.5);

        let config = load_config_or_default();

        Self {
            config,
            status: String::new(),
        }
    }

    fn save(&mut self) {
        let config_path = get_config_file_path();
        let Some(parent) = config_path.parent() else {
            self.status = "Invalid config path.".into();
            return;
        };

        if let Err(err) = fs::create_dir_all(parent) {
            self.status = format!("Could not create config directory: {err}");
            return;
        }

        let text = match toml::to_string_pretty(&self.config) {
            Ok(text) => text,
            Err(err) => {
                self.status = format!("Could not serialize config: {err}");
                return;
            }
        };

        match fs::write(config_path, text) {
            Ok(_) => self.status = "Saved.".to_owned(),
            Err(err) => self.status = format!("Could not save config: {err}"),
        }
    }

    fn reset(&mut self) {
        self.config = Default::default();
        self.status = "Restored defaults. Not saved yet.".into();
    }
}

impl eframe::App for ConfigApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.label("Web UI URL");
            if ui.text_edit_singleline(&mut self.config.ui_url).changed() {
                self.status = UNSAVED_NOTIFICATION.into();
            }
            ui.separator();
            ui.label("Rocket League API Port");
            if ui
                .add(
                    egui::DragValue::new(&mut self.config.rl_api_port)
                        .range(1..=u16::MAX)
                        .speed(1),
                )
                .changed()
            {
                self.status = UNSAVED_NOTIFICATION.into();
            }
            ui.separator();
            if ui
                .checkbox(
                    &mut self.config.start_ui_when_rl_closes,
                    "Start Web UI when Rocket League closes",
                )
                .changed()
            {
                self.status = UNSAVED_NOTIFICATION.into();
            }

            ui.separator();
            ui.horizontal(|ui| {
                if ui.button("Save").clicked() {
                    self.save();
                }

                if ui.button("Reset to default").clicked() {
                    self.reset();
                }
            });

            if !self.status.is_empty() {
                ui.add_space(8.0);
                ui.label(&self.status);
            }
        });
    }
}

fn main() -> eframe::Result {
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default()
            .with_title("Roggo Agent Settings")
            .with_inner_size([500.0, 290.0])
            .with_min_inner_size([500.0, 290.0])
            .with_resizable(true),
        ..Default::default()
    };

    eframe::run_native(
        "Roggo Agent Settings",
        options,
        Box::new(|cc| Ok(Box::new(ConfigApp::new(cc)))),
    )
}
