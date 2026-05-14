use std::sync::{Arc, Mutex};

use eframe::egui;

use crate::core::{
    contract::SimpleMatchDto,
    time::{format_ms_date, format_ms_min_seconds, format_ms_time},
    ui::tasks,
};

#[derive(Default)]
pub struct Content {
    pub matches: Option<Vec<SimpleMatchDto>>,
}

#[derive(Default)]
pub struct MatchOverviewUi {
    content: Arc<Mutex<Content>>,
}

enum NavBarMatchType {
    Won,
    Lost,
    Unknown,
}

impl MatchOverviewUi {
    pub fn update(&self, ui: &mut eframe::egui::Ui) {
        egui::Panel::left("match_list")
        .resizable(false)
        .default_size(150.)
        .show_inside(ui, |ui| {
            egui::ScrollArea::vertical()
                .auto_shrink([false, false])
                .show(ui, |ui| {
                    if let Ok(content) = self.content.lock() {
                        if let Some(matches) = &content.matches {
                            for match_dto in matches {
                                if nav_button(ui, match_dto).clicked() {

                                }
                            }
                        }
                    }
                });
        });
    }

    pub fn reload(&self) {
        tasks::load_matches(self.content.clone());
    }
}

fn get_match_type(match_dto: &SimpleMatchDto) -> NavBarMatchType {
    if match_dto.own_team_score > match_dto.enemy_team_score {
        return NavBarMatchType::Won;
    }
    if match_dto.own_team_score < match_dto.enemy_team_score {
        return NavBarMatchType::Lost;
    }
    return NavBarMatchType::Unknown;
}

fn get_match_queue(match_dto: &SimpleMatchDto) -> String {
    format!(
        "{}v{}",
        match_dto.own_player_count, match_dto.enemy_player_count
    )
}

fn nav_button(ui: &mut egui::Ui, match_dto: &SimpleMatchDto) -> egui::Response {
    let queue = get_match_queue(match_dto);
    let ended_at_date = format_ms_date(match_dto.ended_at);
    let ended_at_time = format_ms_time(match_dto.ended_at);
    let duration = format_ms_min_seconds(match_dto.duration);

    let own_team_score = match_dto.own_team_score;
    let enemy_team_score = match_dto.enemy_team_score;

    let fill = match get_match_type(match_dto) {
        NavBarMatchType::Won => egui::Color32::from_rgb(52, 125, 70),
        NavBarMatchType::Lost => egui::Color32::from_rgb(150, 65, 65),
        NavBarMatchType::Unknown => egui::Color32::from_rgb(96, 96, 104),
    };

    let desired_size = egui::vec2(ui.available_width() - 5.0, 84.0);
    let (rect, response) = ui.allocate_exact_size(desired_size, egui::Sense::click());

    if ui.is_rect_visible(rect) {
        let visuals = ui.style().interact(&response);

        let fill = if response.hovered() {
            fill.gamma_multiply(1.15)
        } else {
            fill
        };

        ui.painter().rect(
            rect,
            visuals.corner_radius,
            fill,
            visuals.bg_stroke,
            egui::StrokeKind::Outside,
        );

        let padding = 8.0;

        let top_y = rect.top() + padding;
        let center_y = rect.center().y;
        let bottom_y = rect.bottom() - padding - 14.0;

        let text_color = egui::Color32::WHITE;
        let weak_text_color = egui::Color32::from_rgb(220, 220, 220);

        ui.painter().text(
            egui::pos2(rect.left() + padding, top_y),
            egui::Align2::LEFT_TOP,
            ended_at_time,
            egui::FontId::proportional(13.0),
            weak_text_color,
        );

        ui.painter().text(
            egui::pos2(rect.left() + padding, bottom_y),
            egui::Align2::LEFT_TOP,
            ended_at_date,
            egui::FontId::proportional(13.0),
            weak_text_color
        );

        ui.painter().text(
            egui::pos2(rect.right() - padding, top_y),
            egui::Align2::RIGHT_TOP,
            duration,
            egui::FontId::proportional(13.0),
            weak_text_color,
        );

        ui.painter().text(
            egui::pos2(rect.right() - padding, bottom_y),
            egui::Align2::RIGHT_TOP,
            queue,
            egui::FontId::proportional(13.0),
            weak_text_color,
        );

        ui.painter().text(
            egui::pos2(rect.center().x, center_y),
            egui::Align2::CENTER_CENTER,
            format!("{own_team_score} : {enemy_team_score}"),
            egui::FontId::proportional(24.0),
            text_color,
        );
    }

    response
}