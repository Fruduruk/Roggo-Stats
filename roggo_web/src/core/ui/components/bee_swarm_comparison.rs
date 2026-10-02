use crate::core::ui::widgets::beeswarm_plot;
use eframe::egui::{self, vec2};
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

    let max = max_value * 1.1;

    ui.horizontal(|ui| {
        for values in player_values {
            ui.vertical(|ui| {
                ui.label(&values.display_name);

                beeswarm_plot::ui(ui, &values.values, vec2(100.0, 200.0), 0.0, max);
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
