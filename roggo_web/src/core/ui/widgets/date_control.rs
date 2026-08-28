use eframe::egui::{self, Widget};
use jiff::civil::Date;

pub fn date_control(ui: &mut egui::Ui, date: &mut Date) -> egui::Response {
    let today = jiff::Zoned::now().date();
    let response = egui_extras::DatePickerButton::new(date)
        .arrows(false)
        .highlight_weekends(false)
        .show_icon(false)
        .format("%d.%m.%Y")
        .reverse_years(true)
        .calendar_week(false)
        .start_end_years(2026..=2026)
        .ui(ui);


    if *date > today {
        *date = today;
    }
    response
}
