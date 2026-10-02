use crate::core::{
    api_result::APIResult,
    tasks,
    ui::{
        components::tab_control::Tab,
        widgets::date_control::{self},
    },
};
use eframe::egui::{self};
use futures_channel::mpsc::Sender;
use jiff::civil::Date;

pub fn ui(
    ui: &mut egui::Ui,
    days_played: &Option<Vec<Date>>,
    sender: &Sender<APIResult>,
    new_tab: &mut Option<Tab>,
    date: &mut Date,
) {
    ui.horizontal(|ui| {
        if let Some(days_played) = days_played.as_ref()
            && let Some(last) = days_played.last()
        {
            let current_date = *date;
            let days_played_index = days_played
                .iter()
                .position(|d| d == date)
                .unwrap_or(days_played.len() - 1);

            if ui.button("←").clicked() {
                let new_date = days_played
                    .get(days_played_index.saturating_sub(1))
                    .unwrap_or(&current_date);
                *date = *new_date;
            }

            date_control::ui(ui, date);

            if !days_played.contains(date) {
                *date = *last;
            }

            if ui.button("→").clicked() {
                let new_date = days_played
                    .get(days_played_index + 1)
                    .unwrap_or(&current_date);
                *date = *new_date;
            }

            if current_date != *date {
                tasks::load_day(sender.clone(), *date);
                *new_tab = Some(Tab::Day);
            }
        }
    });
}
