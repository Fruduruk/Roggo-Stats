use std::time::Duration;

use crate::core::{
    api_result::APIResult,
    app_state::{AppState, agent_state::AgentState},
    contract::AgentErrorDto,
    tasks,
    ui::install_ui::InstallUi,
};
use eframe::egui;
use futures_channel::mpsc::{self, Receiver, Sender};
const GITHUB_URL: &str = "https://github.com/Fruduruk/Roggo-Stats";
pub const UI_VERSION: &str = env!("CARGO_PKG_VERSION");
pub const COMPATIBLE_AGENT_VERSION: &str = "0.7.0";

#[derive(Default)]
pub struct Content {
    pub player_name: Option<String>,
    pub current_error: Option<AgentErrorDto>,
    pub agent_version: Option<String>,
}

pub struct RoggoApp {
    content_sender: Sender<APIResult>,
    content_receiver: Receiver<APIResult>,
    last_reload: f64,
    install_ui: InstallUi,
    state: AppState,
}

impl RoggoApp {
    pub fn new(cc: &eframe::CreationContext<'_>) -> Self {
        cc.egui_ctx.set_pixels_per_point(2.0);

        let (sender, receiver) = mpsc::channel(1000);
        tasks::load_main_character(sender.clone());
        tasks::load_version(sender.clone());

        RoggoApp {
            content_sender: sender,
            content_receiver: receiver,
            last_reload: Default::default(),
            install_ui: Default::default(),
            state: Default::default(),
        }
    }

    fn reload_cycle(&mut self, ui: &mut egui::Ui) {
        ui.ctx().request_repaint_after(Duration::from_secs(1));

        let now = ui.ctx().input(|i| i.time);
        if self.last_reload + 1.0 < now {
            if self.state.player_name.is_none() {
                tasks::load_main_character(self.content_sender.clone());
                tasks::load_version(self.content_sender.clone());
            }
            self.last_reload = now;
        }
    }

    fn main_ui(&mut self, ui: &mut egui::Ui) {
        egui::CentralPanel::default().show(ui, |ui| {
        });
    }
}

impl eframe::App for RoggoApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        self.reload_cycle(ui);

        if let Ok(result) = self.content_receiver.try_recv() {
            self.state.insert(result);
        }

        let agent_version = match &self.state.agent_state {
            AgentState::AgentOutdated(version) | AgentState::Ready(version) => Some(version),
            _ => None,
        };

        egui::Panel::top("header").show(ui, |ui| {
            ui.horizontal(|ui| {
                ui.heading("Roggo Stats");

                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    show_github_button(ui);

                    ui.separator();

                    if let Some(name) = &self.state.player_name {
                        ui.label(name);
                    }

                    ui.separator();

                    if agent_version.is_some() {
                        ui.label(format!("WebUi version {UI_VERSION}"));
                    }
                });
            });
        });

        if matches!(self.state.agent_state, AgentState::CheckingAgent) {
            egui::CentralPanel::default().show(ui, |ui| {
                ui.label("Loading...");
            });

            return;
        }

        match agent_version {
            Some(version) if version == COMPATIBLE_AGENT_VERSION => {
                self.main_ui(ui);
            }
            version => {
                self.install_ui.ui(ui, version.cloned().unwrap_or_default());
            }
        }
    }
}

fn show_github_button(ui: &mut egui::Ui) {
    let image = egui::Image::new(egui::include_image!("../../assets/github.png"))
        .fit_to_exact_size(egui::vec2(16.0, 16.0));
    let response = ui
        .add(
            egui::Button::image(image)
                .min_size(egui::vec2(24.0, 24.0))
                .corner_radius(egui::CornerRadius::same(12))
                .frame(true)
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
