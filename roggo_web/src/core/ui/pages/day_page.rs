use eframe::egui;
use futures_channel::mpsc::Sender;
use uuid::Uuid;

use crate::core::{
    api_result::APIResult, contract::DayDto, tasks, ui::{components::{full_panel::FullPanel, tab_control::Tab}, widgets::session_card},
};

#[derive(Default)]
pub struct DayPage {}

impl DayPage {
    pub fn ui(
        &mut self,
        ui: &mut egui::Ui,
        day_dto: &DayDto,
        sender: &Sender<APIResult>,
        session_match_list: &mut Vec<Uuid>,
        selected_tab: &mut Tab
    ) {
        FullPanel.show(ui, |ui| {
            egui::ScrollArea::vertical().show(ui, |ui| {
                ui.columns(2, |columns| {
                    let left_column_ui = &mut columns[0];
                    self.show_session_cards(left_column_ui, day_dto, sender, session_match_list, selected_tab);
                    let right_column_ui = &mut columns[1];
                    right_column_ui.heading("Day Stats");
                });
            });
        });
    }

    fn show_session_cards(
        &self,
        ui: &mut egui::Ui,
        day_dto: &DayDto,
        sender: &Sender<APIResult>,
        session_match_list: &mut Vec<Uuid>,
        selected_tab: &mut Tab
    ) {
        for session in &day_dto.sessions {
            if session_card::ui(ui, session).clicked() {
                session_match_list.clear();
                session_match_list.extend(session.matches.iter().map(|s| s.match_guid));
                tasks::load_detailed_session(sender.clone(), session_match_list.clone());
                *selected_tab = Tab::Session;
            }
        }
    }
}
