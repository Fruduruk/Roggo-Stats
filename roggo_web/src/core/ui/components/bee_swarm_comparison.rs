use crate::core::ui::widgets::beeswarm_plot;
use eframe::egui::{self, FontId, RichText, vec2};
use roggo_contract::{full::Player, *};

pub struct PlayerValues {
    pub primary_id: String,
    pub display_name: String,
    pub values: Vec<f32>,
}

pub fn ui(
    ui: &mut egui::Ui,
    session_details: &SessionDetails,
    main_character: &PlayerDto,
    value_of: impl Fn(&Player) -> f32,
) {
    let player_values = create_player_values(session_details, main_character, value_of);

    let Some(max_value) = player_values
        .iter()
        .flat_map(|pv| pv.values.iter())
        .copied()
        .max_by(|a, b| a.total_cmp(b))
    else {
        return;
    };

    let max = (max_value * 1.1).max(0.0001);

    let plot_size = vec2(100.0, 200.0);

    ui.horizontal_centered(|ui| {
        ui.vertical(|ui| {
            ui.allocate_ui_with_layout(
                vec2(30.0, plot_size.y),
                egui::Layout::top_down(egui::Align::RIGHT),
                |ui| {
                    let accuracy = if max < 10.0 {
                        2
                    } else if max < 1000.0 {
                        1
                    } else {
                        0
                    };
                    ui.label(
                        RichText::new(format!("{max:.accuracy$}"))
                            .font(FontId::new(10.0, egui::FontFamily::Proportional)),
                    );

                    ui.with_layout(egui::Layout::bottom_up(egui::Align::RIGHT), |ui| {
                        ui.label(
                            RichText::new("0")
                                .font(FontId::new(10.0, egui::FontFamily::Proportional)),
                        );
                    });
                },
            );

            ui.label("");
        });

        for values in player_values {
            ui.vertical(|ui| {
                beeswarm_plot::ui(ui, &values.values, plot_size, 0.0, max);

                ui.label(&values.display_name);
            });
        }
    });
}

fn create_player_values(
    session_details: &SessionDetails,
    main_character: &PlayerDto,
    value_of: impl Fn(&Player) -> f32,
) -> Vec<PlayerValues> {
    let mut result = vec![];

    let mut allies = Vec::<PlayerValues>::new();

    for m in &session_details.matches {
        let Some(main_team) = m.get_team_of_player_by_primary_id(&main_character.primary_id) else {
            continue;
        };

        for player in &main_team.players {
            if let Some(existing) = allies
                .iter_mut()
                .find(|p| p.primary_id == player.primary_id)
            {
                existing.values.push(value_of(player));
            } else {
                allies.push(PlayerValues {
                    primary_id: player.primary_id.clone(),
                    display_name: player.display_name.clone(),
                    values: vec![value_of(player)],
                });
            }
        }
    }

    result.extend(allies);

    let enemy_values = session_details
        .matches
        .iter()
        .filter_map(|m| {
            let enemy_team =
                m.get_enemy_team_of_player_by_primary_id(&main_character.primary_id)?;

            Some(
                enemy_team.players.iter().map(&value_of).sum::<f32>()
                    / enemy_team.players.len() as f32,
            )
        })
        .collect();

    result.push(PlayerValues {
        primary_id: String::new(),
        display_name: "Average Enemy".into(),
        values: enemy_values,
    });

    result
}
