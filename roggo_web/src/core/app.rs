use std::time::Duration;

use crate::core::{
    api_result::APIResult,
    app_state::{AppState, agent_state::AgentState},
    contract::AgentErrorDto,
    tasks,
    ui::{
        components::{footer, header},
        install_ui::InstallUi,
        pages::development_page::DevelopmentPage,
        theme::apply_theme,
    },
};
use eframe::egui;
use futures_channel::mpsc::{self, Receiver, Sender};
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
    development_page: DevelopmentPage,
}

impl RoggoApp {
    pub fn new(cc: &eframe::CreationContext<'_>) -> Self {
        apply_theme(&cc.egui_ctx);

        let (sender, receiver) = mpsc::channel(1000);
        tasks::load_main_character(sender.clone());
        tasks::load_version(sender.clone());

        RoggoApp {
            content_sender: sender,
            content_receiver: receiver,
            last_reload: Default::default(),
            install_ui: Default::default(),
            state: Default::default(),
            development_page: Default::default(),
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
        self.development_page.ui(ui, &mut self.state);
    }
}

impl eframe::App for RoggoApp {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        self.reload_cycle(ui);

        if let Ok(result) = self.content_receiver.try_recv() {
            self.state.insert(result);
        }

        let agent_version = match &self.state.agent_state {
            AgentState::AgentOutdated(version) | AgentState::Ready(version) => {
                Some(version.clone())
            }
            _ => None,
        };

        header::ui(ui, &mut self.state.parameters.date, &self.state.player_name);

        footer::ui(ui, agent_version.clone());

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
                self.install_ui.ui(ui, version.unwrap_or_default());
            }
        }
    }
}
