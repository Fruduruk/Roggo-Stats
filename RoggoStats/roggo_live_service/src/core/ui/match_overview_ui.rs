use eframe::egui;


#[derive(Default)]
pub struct MatchOverviewUi {
    
}

impl MatchOverviewUi {
    pub fn update(&self, ui: &mut eframe::egui::Ui) {
        egui::Panel::left("match_list").show_inside(ui, |ui| {
            if nav_button(ui,"Live",true).clicked() {

            }
        });
    }
}

fn nav_button(ui: &mut egui::Ui, text: &str, selected: bool) -> egui::Response {
    let button = egui::Button::new(text)
        .selected(selected)
        .min_size(egui::vec2(ui.available_width(), 36.0));

    ui.add(button)
}