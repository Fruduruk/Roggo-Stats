#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::{fs, path::PathBuf, process::Command};

use eframe::egui;
use roggo_agent::{AgentConfig, get_config_file_path, load_config_or_default};
const UNSAVED_NOTIFICATION: &str = "Not saved yet.";

use serde::Deserialize;

#[derive(Debug, Deserialize)]
pub struct TAStatsAPI {
    #[serde(rename = "TAGame")]
    pub ta_game: TaGame,
}

impl TAStatsAPI {
    pub fn get_port(&self) -> u16 {
        self.ta_game.match_stats_exporter.port
    }

    pub fn get_packet_send_rate(&self) -> u32 {
        self.ta_game.match_stats_exporter.packet_send_rate
    }
}

#[derive(Debug, Deserialize)]
pub struct TaGame {
    #[serde(rename = "MatchStatsExporter_TA")]
    pub match_stats_exporter: MatchStatsExporter,
}

#[derive(Debug, Deserialize)]
pub struct MatchStatsExporter {
    #[serde(rename = "Port")]
    pub port: u16,

    #[serde(rename = "PacketSendRate")]
    pub packet_send_rate: u32,
}

struct ConfigApp {
    config: AgentConfig,
    rl_config: Option<TAStatsAPI>,
    status: String,
    notepad_process: Option<std::process::Child>,
}

impl ConfigApp {
    fn new(cc: &eframe::CreationContext) -> Self {
        cc.egui_ctx.set_pixels_per_point(1.5);

        let config = load_config_or_default();
        let rl_config = load_rl_config();

        Self {
            config,
            rl_config,
            notepad_process: None,
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
        if let Some(child) = &mut self.notepad_process {
            if let Ok(exit_code) = child.try_wait() {
                if exit_code.is_some() {
                    self.rl_config = load_rl_config();
                }
            }
        }

        egui::Panel::bottom("bottom_panel")
            .default_size(100.)
            .show_inside(ui, |ui| {
                if !self.status.is_empty() {
                    ui.label(&self.status);
                }
            });

        egui::Panel::right("rocket_league_config")
            .resizable(false)
            .default_size(250.)
            .show_inside(ui, |ui| {
                ui.heading("Rocket League config file settings");
                if let Some(config) = &self.rl_config {
                    let port_color = if self.config.rl_api_port != config.get_port() {
                        egui::Color32::RED
                    } else {
                        egui::Color32::GREEN
                    };

                    let packet_send_rate_color = if config.get_packet_send_rate() != 120 {
                        egui::Color32::RED
                    } else {
                        egui::Color32::GREEN
                    };

                    ui.label(
                        egui::RichText::new(format!("Sender Port: {}", config.get_port()))
                            .color(port_color),
                    );
                    ui.label(
                        egui::RichText::new(format!(
                            "PacketSendRate: {}",
                            config.get_packet_send_rate()
                        ))
                        .color(packet_send_rate_color),
                    );
                } else {
                    ui.label("Could not read Rocket League config");
                }
                ui.separator();

                ui.with_layout(egui::Layout::bottom_up(egui::Align::Center), |ui| {
                    if ui.button("Open config with notepad").clicked() {
                        if let Some(config_path) = get_rocket_league_api_config_path() {
                            let result = Command::new("notepad").arg(config_path).spawn();
                            match result {
                                Ok(child) => self.notepad_process = Some(child),
                                Err(error) => {
                                    self.status = format!("Could not open config file: {error}")
                                }
                            }
                        }
                    }
                });
            });

        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.heading("Roggo Agent Settings");
            ui.horizontal(|ui| {
                ui.label("Listen Port:");
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
            });

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

            ui.with_layout(egui::Layout::bottom_up(egui::Align::Center), |ui| {
                ui.horizontal(|ui| {
                    if ui.button("Save").clicked() {
                        self.save();
                    }

                    if ui.button("Reset to default").clicked() {
                        self.reset();
                    }
                });
            });
        });
    }
}

fn get_rocket_league_api_config_path() -> Option<PathBuf> {
    Some(dirs::document_dir()?.join("My Games\\Rocket League\\TAGame\\Config\\TAStatsAPI.ini"))
}

fn main() -> eframe::Result {
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default()
            .with_title("Roggo Agent Settings")
            .with_inner_size([700.0, 290.0])
            .with_min_inner_size([700.0, 290.0])
            .with_resizable(true)
            .with_icon({
                let bytes = include_bytes!("../../assets/icon.ico");
                let image = image::load_from_memory(bytes)
                    .expect("Failed to load icon")
                    .to_rgba8();
                egui::IconData {
                    width: image.width(),
                    height: image.height(),
                    rgba: image.into_raw(),
                }
            }),
        ..Default::default()
    };

    eframe::run_native(
        "Roggo Agent Settings",
        options,
        Box::new(|cc| Ok(Box::new(ConfigApp::new(cc)))),
    )
}

pub fn load_rl_config() -> Option<TAStatsAPI> {
    Some(
        fs::read_to_string(get_rocket_league_api_config_path()?)
            .ok()
            .and_then(|text| toml::from_str(&text).ok())?,
    )
}
