use std::time::Duration;

use crate::core::{
    api_result::APIResult,
    app_state::{AppState, agent_state::AgentState},
    contract::AgentErrorDto,
    tasks,
    ui::{
        components::{
            footer, header,
            tab_control::{Tab, TabControl},
        },
        install_ui::InstallUi,
        pages::{
            all_time_page::AllTimePage, day_page::DayPage, match_page::MatchPage,
            session_page::SessionPage,
        },
        theme::{apply_theme, colors::colors},
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
    tab_control: TabControl,
    state: AppState,
    // development_page: DevelopmentPage,
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

        header::ui(
            ui,
            &self.state.player_name,
            &mut self.state.parameters.date,
            &self.content_sender,
            &mut self.tab_control.selected,
        );

        footer::ui(ui, agent_version.clone());

        if matches!(self.state.agent_state, AgentState::CheckingAgent) {
            egui::CentralPanel::default().show(ui, |ui| {
                ui.with_layout(
                    egui::Layout::centered_and_justified(egui::Direction::TopDown),
                    |ui| {
                        ui.add(egui::Spinner::new().size(24.0));
                    },
                );
            });

            return;
        }

        match agent_version {
            Some(version) if version == COMPATIBLE_AGENT_VERSION => {
                self.show_central_panel(ui);
            }
            version => {
                self.install_ui.ui(ui, version.unwrap_or_default());
            }
        }
    }
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
            // development_page: Default::default(),
            tab_control: Default::default(),
        }
    }

    fn reload_cycle(&mut self, ui: &mut egui::Ui) {
        ui.ctx().request_repaint_after(Duration::from_secs(1));

        let now = ui.ctx().input(|i| i.time);
        if self.last_reload + 1.0 < now {
            tasks::load_day(self.content_sender.clone(), self.state.parameters.date);

            if self.state.player_name.is_none() {
                tasks::load_main_character(self.content_sender.clone());
                tasks::load_version(self.content_sender.clone());
            }
            self.last_reload = now;
        }
    }

    fn show_central_panel(&mut self, ui: &mut egui::Ui) {
        egui::CentralPanel::default()
            .frame(egui::Frame::new().fill(colors(ui).background))
            .show(ui, |ui| match self.tab_control.ui(ui) {
                (Tab::Day, changed) => {
                    if changed {
                        tasks::load_day(self.content_sender.clone(), self.state.parameters.date);
                    }
                    if let Some(day) = &self.state.day {
                        DayPage::default().ui(
                            ui,
                            day,
                            &self.content_sender,
                            &mut self.state.parameters.session_match_list,
                            &mut self.tab_control.selected,
                        );
                    }
                }
                (Tab::Session, _changed) => {
                    if let Some(detailed_session) = &self.state.session {
                        SessionPage::default().ui(ui, detailed_session);
                    }
                }
                (Tab::Match, _changed) => MatchPage::default().ui(ui),
                (Tab::AllTime, _changed) => AllTimePage::default().ui(ui),
            });
    }
}
