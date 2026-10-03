use eframe::egui::{self, FontId, RichText, Stroke, Vec2, pos2, vec2};
use roggo_contract::{PlayerDto, SessionDetails};
use uuid::Uuid;

use crate::core::ui::{
    components::bee_swarm_comparison::{Filter, create_display_value},
    theme::colors::{ThemeColors, colors},
    widgets::beeswarm_plot::DisplayValue,
};

const LABEL_WIDTH: f32 = 90.0;
const ROW_HEIGHT: f32 = 36.0;
const SCALE_HEIGHT: f32 = 16.0;

const RADIUS: f32 = 4.0;
const SPACING: f32 = RADIUS * 2.0;

pub fn ui(ui: &mut egui::Ui, my_team: &[DisplayValue], enemy_team: &[DisplayValue]) {
    let spacing = ui.spacing().item_spacing.x;
    let plot_width = (ui.available_width() - LABEL_WIDTH - spacing).max(100.0);

    draw_scale(ui, plot_width);

    draw_row(ui, "My Team", my_team, vec2(plot_width, ROW_HEIGHT));

    draw_row(ui, "Enemy Team", enemy_team, vec2(plot_width, ROW_HEIGHT));
}

fn draw_scale(ui: &mut egui::Ui, plot_width: f32) {
    ui.horizontal(|ui| {
        ui.allocate_space(vec2(LABEL_WIDTH, SCALE_HEIGHT));

        let (rect, _) =
            ui.allocate_exact_size(vec2(plot_width, SCALE_HEIGHT), egui::Sense::hover());

        let painter = ui.painter_at(rect);
        let font = FontId::new(10.0, egui::FontFamily::Proportional);

        painter.text(
            pos2(rect.left(), rect.bottom()),
            egui::Align2::LEFT_BOTTOM,
            "0",
            font.clone(),
            colors(ui).on_panel,
        );

        painter.text(
            pos2(rect.right(), rect.bottom()),
            egui::Align2::RIGHT_BOTTOM,
            "1",
            font,
            colors(ui).on_panel,
        );
    });
}

fn draw_row(ui: &mut egui::Ui, label: &str, values: &[DisplayValue], size: Vec2) {
    ui.horizontal(|ui| {
        ui.allocate_ui_with_layout(
            vec2(LABEL_WIDTH, size.y),
            egui::Layout::centered_and_justified(egui::Direction::LeftToRight),
            |ui| {
                ui.label(
                    RichText::new(label).font(FontId::new(11.0, egui::FontFamily::Proportional)),
                );
            },
        );

        draw_plot(ui, values, size);
    });
}

fn draw_plot(ui: &mut egui::Ui, values: &[DisplayValue], size: Vec2) {
    let (rect, _) = ui.allocate_exact_size(size, egui::Sense::hover());

    let painter = ui.painter_at(rect);

    painter.rect_stroke(
        rect,
        3.0,
        Stroke::new(1.0, colors(ui).border),
        egui::StrokeKind::Inside,
    );

    let y = rect.center().y;

    painter.line_segment(
        [pos2(rect.left(), y), pos2(rect.right(), y)],
        Stroke::new(1.0, colors(ui).on_panel),
    );

    if values.is_empty() {
        return;
    }

    let average = values.iter().map(|v| v.value).sum::<f32>() / values.len() as f32;

    let average_x = egui::remap(average, 0.0..=1.0, rect.left()..=rect.right());

    painter.line_segment(
        [pos2(average_x, rect.top()), pos2(average_x, rect.bottom())],
        Stroke::new(1.0, colors(ui).accent),
    );

    painter.text(
        pos2(average_x + 3.0, rect.top() + 2.0),
        egui::Align2::LEFT_TOP,
        format!("{average:.2}"),
        FontId::new(10.0, egui::FontFamily::Proportional),
        colors(ui).accent,
    );

    let mut placed = Vec::<egui::Pos2>::new();

    for &display_value in values {
        let x = egui::remap(
            display_value.value.clamp(0.0, 1.0),
            0.0..=1.0,
            rect.left()..=rect.right(),
        );

        let mut step = 0;

        let pos = loop {
            let offset = match step {
                0 => 0.0,

                n if n % 2 == 1 => ((n + 1) / 2) as f32 * SPACING,

                n => -(n / 2) as f32 * SPACING,
            };

            let pos = pos2(x, y + offset);

            if placed.iter().all(|p| p.distance(pos) >= SPACING) {
                break pos;
            }

            step += 1;
        };

        painter.circle_filled(pos, RADIUS, display_value.color);

        if display_value.emphasized {
            painter.circle(
                pos,
                RADIUS + 3.0,
                display_value.color.gamma_multiply(1.2),
                Stroke::new(2.0, colors(ui).on_panel),
            );
        }

        placed.push(pos);
    }
}
pub fn create_mate_score_difference_values(
    session_details: &SessionDetails,
    main_character: &PlayerDto,
    filter: Filter,
    selected_match_guid: Option<Uuid>,
    colors: ThemeColors,
) -> (Vec<DisplayValue>, Vec<DisplayValue>) {
    let Some(min) = session_details.matches.iter().map(|m| m.created_at).min() else {
        return (vec![], vec![]);
    };

    let Some(max) = session_details.matches.iter().map(|m| m.created_at).max() else {
        return (vec![], vec![]);
    };

    let total_ms = max - min;

    let mut my_team = vec![];
    let mut enemy_team = vec![];

    for m in &session_details.matches {
        let percent_of_total = if total_ms == 0 {
            0.0
        } else {
            (m.created_at - min) as f32 / total_ms as f32
        };

        let alpha = egui::lerp(0.0..=255.0, percent_of_total).floor() as u8;

        let Some(team) = m.get_team_of_player_by_primary_id(&main_character.primary_id) else {
            continue;
        };

        let Some(enemy) = m.get_enemy_team_of_player_by_primary_id(&main_character.primary_id)
        else {
            continue;
        };

        if let Some(value) = team.mate_score_difference() {
            my_team.push(create_display_value(
                main_character,
                filter,
                selected_match_guid,
                colors,
                m,
                alpha,
                value as f32,
            ));
        }

        if let Some(value) = enemy.mate_score_difference() {
            enemy_team.push(create_display_value(
                main_character,
                filter,
                selected_match_guid,
                colors,
                m,
                alpha,
                value as f32,
            ));
        }
    }

    (my_team, enemy_team)
}
