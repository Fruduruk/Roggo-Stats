use std::sync::{Arc, Mutex};

use eframe::egui::{self, Context};

use crate::core::{
    contract::{SimpleMatchDto, SimpleSessionDto},
    time::{format_ms_date, format_ms_min_seconds, format_ms_time},
    ui::{match_ui::MatchUi, session_ui::SessionUi, tasks},
};

pub const RED: egui::Color32 = egui::Color32::from_rgb(150, 65, 65);
pub const YELLOW: egui::Color32 = egui::Color32::from_rgb(180, 160, 70);
pub const GREEN: egui::Color32 = egui::Color32::from_rgb(52, 125, 70);
pub const GREY: egui::Color32 = egui::Color32::from_rgb(96, 96, 104);

#[derive(Default)]
pub struct Content {
    pub matches: Option<Vec<SimpleMatchDto>>,
    pub sessions: Option<Vec<SimpleSessionDto>>,
}

pub struct MatchOverviewUi {
    match_ui: MatchUi,
    session_ui: SessionUi,
    view_mode: ViewMode,
    content: Arc<Mutex<Content>>,
    full_reload_requested: Arc<Mutex<bool>>,
}

#[derive(Default, PartialEq, Eq)]
enum ViewMode {
    #[default]
    SingleMatch,
    Session(i64),
}

enum NavBarMatchType {
    Won,
    Lost,
    Unknown,
}

impl MatchOverviewUi {
    pub fn new_with_single_reload_arc(arc: Arc<Mutex<bool>>) -> Self {
        Self {
            match_ui: MatchUi::new_with_single_reload_arc(arc.clone()),
            session_ui: SessionUi::new_with_single_reload_arc(arc.clone()),
            view_mode: Default::default(),
            content: Default::default(),
            full_reload_requested: arc,
        }
    }

    pub fn ui(&mut self, ui: &mut eframe::egui::Ui, player_name: &String) {
        egui::Panel::left("match_list")
            .resizable(false)
            .default_size(150.)
            .show_inside(ui, |ui| {
                ui.heading("Matches");
                ui.separator();
                egui::ScrollArea::vertical()
                    .auto_shrink([false, false])
                    .show(ui, |ui| {
                        if let Ok(content) = self.content.lock() {
                            if let Some(matches) = &content.matches {
                                for match_dto in matches {
                                    if match_button(ui, match_dto).clicked() {
                                        self.view_mode = ViewMode::SingleMatch;
                                        self.match_ui
                                            .reload(ui.ctx().clone(), match_dto.match_guid);
                                    }
                                }
                            }
                        }
                    });
            });

        egui::Panel::right("session_list")
            .resizable(false)
            .default_size(150.)
            .show_inside(ui, |ui| {
                ui.heading("Sessions");
                ui.separator();
                egui::ScrollArea::vertical()
                    .auto_shrink([false, false])
                    .show(ui, |ui| {
                        if let Ok(content) = self.content.lock() {
                            if let Some(sessions) = &content.sessions {
                                for session in sessions {
                                    if session_button(ui, session).clicked() {
                                        self.view_mode = ViewMode::Session(session.created_at);
                                        self.session_ui
                                            .reload(ui.ctx().clone(), session.match_guids.clone());
                                    }
                                }
                            }
                        }
                    });
            });

        match self.view_mode {
            ViewMode::SingleMatch => {
                egui::CentralPanel::default().show_inside(ui, |ui| {
                    self.match_ui.ui(ui);
                });
            }
            ViewMode::Session(_) => {
                egui::CentralPanel::default().show_inside(ui, |ui| {
                    self.session_ui.ui(ui, player_name);
                });
            }
        }
    }

    pub fn reload(&mut self, context: Context) {
        tasks::load_matches(context.clone(), self.content.clone());
        tasks::load_sessions(context.clone(), self.content.clone(), 3_600_000);
        if let ViewMode::Session(created_at) = &self.view_mode {
            let match_guids = {
                let Ok(content) = self.content.lock() else {
                    return;
                };

                let Some(sessions) = &content.sessions else {
                    return;
                };

                let Some(session) = sessions.iter().find(|s| s.created_at == *created_at) else {
                    return;
                };

                session.match_guids.clone()
            };
            self.session_ui.reload(context, match_guids);
        }
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

fn match_button(ui: &mut egui::Ui, match_dto: &SimpleMatchDto) -> egui::Response {
    let queue = format!(
        "{}v{}",
        match_dto.own_player_count, match_dto.enemy_player_count
    );
    let ended_at_date = format_ms_date(match_dto.ended_at);
    let ended_at_time = format_ms_time(match_dto.ended_at);
    let duration = format_ms_min_seconds(match_dto.duration);

    let own_team_score = match_dto.own_team_score;
    let enemy_team_score = match_dto.enemy_team_score;

    let fill = match get_match_type(match_dto) {
        NavBarMatchType::Won => GREEN,
        NavBarMatchType::Lost => RED,
        NavBarMatchType::Unknown => GREY,
    };

    let fill = if match_dto.hidden {
        fill.gamma_multiply(0.5)
    } else {
        fill
    };

    nav_button(
        ui,
        queue,
        ended_at_date,
        ended_at_time,
        duration,
        format!("{own_team_score} : {enemy_team_score}"),
        fill,
    )
}

fn session_button(ui: &mut egui::Ui, session: &SimpleSessionDto) -> egui::Response {
    let queue = format!(
        "{}v{}",
        session.own_player_count, session.enemy_player_count
    );
    let ended_at_date = format_ms_date(session.ended_at);
    let ended_at_time = format_ms_time(session.ended_at);
    let duration = format_ms_min_seconds(session.ended_at - session.created_at);

    let win_rate = if session.match_count == 0 {
        0.5
    } else {
        session.matches_won as f32 / session.match_count as f32
    };

    let fill = if win_rate < 0.5 {
        RED
    } else if win_rate == 0.5 {
        YELLOW
    } else {
        GREEN
    };

    nav_button(
        ui,
        queue,
        ended_at_date,
        ended_at_time,
        duration,
        format!("{} / {}", session.matches_won, session.match_count),
        fill,
    )
}

fn nav_button(
    ui: &mut egui::Ui,
    bottom_right_text: String,
    bottom_left_text: String,
    top_left_text: String,
    top_right_text: String,
    middle_text: String,
    fill: egui::Color32,
) -> egui::Response {
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
            top_left_text,
            egui::FontId::proportional(13.0),
            weak_text_color,
        );

        ui.painter().text(
            egui::pos2(rect.left() + padding, bottom_y),
            egui::Align2::LEFT_TOP,
            bottom_left_text,
            egui::FontId::proportional(13.0),
            weak_text_color,
        );

        ui.painter().text(
            egui::pos2(rect.right() - padding, top_y),
            egui::Align2::RIGHT_TOP,
            top_right_text,
            egui::FontId::proportional(13.0),
            weak_text_color,
        );

        ui.painter().text(
            egui::pos2(rect.right() - padding, bottom_y),
            egui::Align2::RIGHT_TOP,
            bottom_right_text,
            egui::FontId::proportional(13.0),
            weak_text_color,
        );

        ui.painter().text(
            egui::pos2(rect.center().x, center_y),
            egui::Align2::CENTER_CENTER,
            middle_text,
            egui::FontId::proportional(24.0),
            text_color,
        );
    }
    response
}
