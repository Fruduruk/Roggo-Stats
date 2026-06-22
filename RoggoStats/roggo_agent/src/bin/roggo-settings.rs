#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::{fs, path::PathBuf};

use eframe::egui;
use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize)]
struct AgentConfig {
    enabled: bool,
    api_url: String,
}

impl Default for AgentConfig {
    fn default() -> Self {
        Self {
            enabled: true,
            api_url: "https://roggo.frudd.dev".to_owned(),
        }
    }
}

struct ConfigApp {
    config: AgentConfig,
    config_path: PathBuf,
    status: String,
}

impl ConfigApp {
    fn new() -> Self {
        let config_path = config_path();
        let config = load_config(&config_path);

        Self {
            config,
            config_path,
            status: String::new(),
        }
    }

    fn save(&mut self) {
        let Some(parent) = self.config_path.parent() else {
            self.status = "Invalid config path.".to_owned();
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

        match fs::write(&self.config_path, text) {
            Ok(_) => self.status = "Saved.".to_owned(),
            Err(err) => self.status = format!("Could not save config: {err}"),
        }
    }
}

impl eframe::App for ConfigApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.heading("Roggo Agent Settings");

            ui.add_space(8.0);

            ui.checkbox(&mut self.config.enabled, "Enable agent");

            ui.add_space(8.0);

            ui.label("API URL");
            ui.text_edit_singleline(&mut self.config.api_url);

            ui.add_space(12.0);

            ui.horizontal(|ui| {
                if ui.button("Save").clicked() {
                    self.save();
                }

                if ui.button("Close").clicked() {
                    ui.ctx().send_viewport_cmd(egui::ViewportCommand::Close);
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
            .with_inner_size([360.0, 160.0])
            .with_resizable(false),
        ..Default::default()
    };

    eframe::run_native(
        "Roggo Agent Settings",
        options,
        Box::new(|_cc| Ok(Box::new(ConfigApp::new()))),
    )
}

fn config_path() -> PathBuf {
    dirs::config_dir()
        .unwrap_or_else(|| std::env::current_dir().unwrap())
        .join("RoggoStats")
        .join("agent.toml")
}

fn load_config(path: &PathBuf) -> AgentConfig {
    fs::read_to_string(path)
        .ok()
        .and_then(|text| toml::from_str(&text).ok())
        .unwrap_or_default()
}