#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::{collections::HashMap, fs, path::PathBuf, process::Command};

use eframe::egui;

use roggo_agent::settings::{
    get_config_file_path, load_config_or_default,
    models::{AgentConfig, TAStatsAPI},
};

struct ConfigState {
    pub send_rate: (bool, u32),
    pub port: (bool, u16),
}

struct Notification {
    pub text: String,
    pub color: egui::Color32,
}

const WARN_COLOR: egui::Color32 = egui::Color32::LIGHT_RED;
const SUCCESS_COLOR: egui::Color32 = egui::Color32::LIGHT_GREEN;
const INFO_COLOR: egui::Color32 = egui::Color32::YELLOW;

struct ConfigApp {
    config: AgentConfig,
    rl_config: Option<TAStatsAPI>,
    notepad_process: Option<std::process::Child>,
    config_state: Option<ConfigState>,
    notifications: HashMap<String, Notification>,
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
            config_state: None,
            notifications: HashMap::new(),
        }
    }

    fn save(&mut self) {
        let config_path = get_config_file_path();
        let Some(parent) = config_path.parent() else {
            self.display_information("error", "Invalid config path.", WARN_COLOR);
            return;
        };

        if let Err(err) = fs::create_dir_all(parent) {
            self.display_information(
                "error",
                format!("Could not create config directory: {err}"),
                WARN_COLOR,
            );
            return;
        }

        let text = match toml::to_string_pretty(&self.config) {
            Ok(text) => text,
            Err(err) => {
                self.display_information(
                    "error",
                    format!("Could not serialize config: {err}"),
                    WARN_COLOR,
                );
                return;
            }
        };

        match fs::write(config_path, text) {
            Ok(_) => {
                self.display_information("saved", "Saved agent settings.", SUCCESS_COLOR);
                self.remove_information("restore");
                self.remove_information("changed");
            },
            Err(err) => self.display_information(
                "error",
                format!("Could not save config: {err}"),
                WARN_COLOR,
            ),
        }
    }

    fn display_information(
        &mut self,
        key: impl Into<String>,
        text: impl Into<String>,
        color: egui::Color32,
    ) {
        self.notifications.insert(
            key.into(),
            Notification {
                text: text.into(),
                color,
            },
        );
    }

    fn remove_information(&mut self, key: impl Into<String>) {
        self.notifications.remove(&key.into());
    }

    fn reset(&mut self) {
        self.config = Default::default();
        self.display_information("restore", "Restored defaults", INFO_COLOR);
        self.remove_information("saved");
    }

    fn show_agent_settings(&mut self, ui: &mut egui::Ui) {
        ui.add_space(5.5);
        ui.heading("Roggo Agent Settings");
        ui.separator();
        ui.horizontal(|ui| {
            ui.label("Listener Port:");
            let response = ui
                .add(
                    egui::DragValue::new(&mut self.config.rl_api_port)
                        .range(1..=u16::MAX)
                        .speed(1),
                )
                .on_hover_text(
                    "Change on both ends if the default port is already in use by another process.",
                );
            if response.changed() {
                self.display_information("changed", "Not saved yet.", INFO_COLOR);
                self.remove_information("saved");
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
            self.display_information("changed", "Not saved yet.", INFO_COLOR);
            self.remove_information("saved");
        }

        ui.separator();
    }

    fn show_rl_settings(&mut self, ui: &mut egui::Ui) {
        ui.heading("Rocket League");
        ui.separator();
        if let Some(ConfigState {
            send_rate: (send_rate_correct, send_rate),
            port: (port_correct, port),
        }) = &self.config_state
        {
            let port_color = match *port_correct {
                true => egui::Color32::LIGHT_GREEN,
                false => egui::Color32::LIGHT_RED,
            };

            let packet_send_rate_color = match *send_rate_correct {
                true => egui::Color32::LIGHT_GREEN,
                false => egui::Color32::LIGHT_RED,
            };

            ui.label(egui::RichText::new(format!("Sender Port: {}", *port)).color(port_color));
            ui.label(
                egui::RichText::new(format!("PacketSendRate: {}", *send_rate))
                    .color(packet_send_rate_color),
            );
        } else {
            ui.label("Could not read Rocket League config");
        }
        ui.separator();
    }

    fn reload_rl_config_if_notepad_closed(&mut self) {
        let mut remove_handle = false;
        if let Some(child) = &mut self.notepad_process {
            if let Ok(exit_code) = child.try_wait() {
                if exit_code.is_some() {
                    self.rl_config = load_rl_config();
                    remove_handle = true;
                }
            }
        }
        if remove_handle {
            self.display_information(
                "restart rocket league",
                "Make sure to restart Rocket League if you've edited the TAStatsAPI.ini.",
                INFO_COLOR,
            );
            self.notepad_process = None;
        }
    }

    fn reload_config_state(&mut self) {
        self.config_state = self.rl_config.as_ref().map(|rl_config| {
            let send_rate = rl_config.get_packet_send_rate();
            let port = rl_config.get_port();

            let send_rate_correct = send_rate == 120;
            let port_correct = port == self.config.rl_api_port;

            if !port_correct {
                self.notifications.insert(
                    "port incorrect".into(),
                    Notification {
                        text: "Please set the same ports on both ends.".into(),
                        color: egui::Color32::LIGHT_RED,
                    },
                );
            } else {
                self.notifications.remove("port incorrect");
            }

            if !send_rate_correct {
                self.notifications.insert(
                    "rate incorrect".into(),
                    Notification {
                        text: "Please set the packet send rate to 120 to get maximum accuracy"
                            .into(),
                        color: egui::Color32::LIGHT_RED,
                    },
                );
            } else {
                self.notifications.remove("rate incorrect");
            }

            ConfigState {
                send_rate: (send_rate_correct, send_rate),
                port: (port_correct, port),
            }
        });
    }

    fn show_information_panel(&mut self, ui: &mut egui::Ui) {
        ui.add_space(5.5);
        ui.heading("Information");
        ui.separator();
        for (_, notification) in &self.notifications {
            ui.label(egui::RichText::new(&notification.text).color(notification.color));
            ui.separator();
        }
    }

    fn show_bottom_panel(&mut self, ui: &mut egui::Ui) {
        egui::Frame::NONE
            .inner_margin(egui::Margin::same(5))
            .show(ui, |ui| {
                ui.horizontal(|ui| {
                    ui.horizontal(|ui| {
                        if ui
                            .button("Save")
                            .on_hover_text(
                                "Saves to config file and restarts the agent with the new settings",
                            )
                            .clicked()
                        {
                            self.save();
                        }
                        ui.add_space(2.);
                        if ui.button("Reset to default").clicked() {
                            self.reset();
                        }
                        ui.add_space(2.);
                        if ui
                            .button("Open Rocket League config")
                            .on_hover_text(format!(
                                "Opens {} with notepad",
                                get_rocket_league_api_config_path()
                                    .unwrap_or("config not found".into())
                                    .to_string_lossy()
                            ))
                            .clicked()
                        {
                            self.try_open_config_file_in_notepad();
                        }
                    });
                });
            });
    }

    fn try_open_config_file_in_notepad(&mut self) {
        if let Some(config_path) = get_rocket_league_api_config_path() {
            let result = Command::new("notepad").arg(config_path).spawn();
            match result {
                Ok(child) => self.notepad_process = Some(child),
                Err(error) => {
                    self.display_information(
                        "error",
                        format!("Could not open config file: {error}"),
                        WARN_COLOR,
                    );
                }
            }
        }
    }
}

impl eframe::App for ConfigApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        self.reload_rl_config_if_notepad_closed();
        self.reload_config_state();

        egui::Panel::bottom("bottom_panel")
            .default_size(70.)
            .show_inside(ui, |ui| {
                self.show_bottom_panel(ui);
            });

        egui::Panel::left("left_panel")
            .min_size(250.)
            .resizable(false)
            .show_inside(ui, |ui| {
                self.show_agent_settings(ui);
            });

        egui::Panel::right("right_panel")
            .min_size(250.)
            .resizable(false)
            .show_inside(ui, |ui| {
                self.show_information_panel(ui);
            });

        egui::CentralPanel::default().show_inside(ui, |ui| {
            self.show_rl_settings(ui);
        });
    }
}

fn get_rocket_league_api_config_path() -> Option<PathBuf> {
    Some(dirs::document_dir()?.join("My Games\\Rocket League\\TAGame\\Config\\TAStatsAPI.ini"))
}

fn main() -> eframe::Result {
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default()
            .with_title("Roggo Agent")
            .with_inner_size([1000.0, 490.0])
            .with_min_inner_size([1000.0, 490.0])
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
