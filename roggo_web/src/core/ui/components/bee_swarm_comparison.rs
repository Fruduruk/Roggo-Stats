use crate::core::ui::{
    theme::colors::{ThemeColors, colors},
    widgets::beeswarm_plot::{self, DisplayValue},
};
use eframe::egui::{self, FontId, RichText, vec2};

use roggo_contract::{full::Player, *};
use uuid::Uuid;

#[derive(Default, Copy, Clone, PartialEq, enum_iterator::Sequence)]
pub enum Filter {
    #[default]
    NoFilter,
    WinLoss,
    MVP,
    Time,
}

impl std::fmt::Display for Filter {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Filter::NoFilter => write!(f, "None"),
            Filter::WinLoss => write!(f, "Win/Loss"),
            Filter::MVP => write!(f, "MVP"),
            Filter::Time => write!(f, "Time"),
        }
    }
}

#[derive(Default, Copy, Clone, PartialEq, enum_iterator::Sequence)]
pub enum Statistic {
    #[default]
    Score,
    Goals,
    Shots,
    Assists,
    Saves,
    Touches,
    CarTouches,
    Demos,
    PercentBoosting,
    PercentDemolished,
    PercentOnGround,
    PercentOnWall,
    PercentPowersliding,
    PercentSupersonic,
}

impl std::fmt::Display for Statistic {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Statistic::Score => write!(f, "Score"),
            Statistic::Goals => write!(f, "Goals"),
            Statistic::Shots => write!(f, "Shots"),
            Statistic::Assists => write!(f, "Assists"),
            Statistic::Saves => write!(f, "Saves"),
            Statistic::Touches => write!(f, "Touches"),
            Statistic::CarTouches => write!(f, "Car Touches"),
            Statistic::Demos => write!(f, "Demos"),
            Statistic::PercentBoosting => write!(f, "% Boosting"),
            Statistic::PercentDemolished => write!(f, "% Demolished"),
            Statistic::PercentOnGround => write!(f, "% on Ground"),
            Statistic::PercentOnWall => write!(f, "% on Wall"),
            Statistic::PercentPowersliding => write!(f, "% Powersliding"),
            Statistic::PercentSupersonic => write!(f, "% Supersonic"),
        }
    }
}

pub struct PlayerValues {
    pub primary_id: String,
    pub display_name: String,
    pub values: Vec<DisplayValue>,
}

pub fn ui(
    ui: &mut egui::Ui,
    session_details: &SessionDetails,
    main_character: &PlayerDto,
    statistic: (Statistic, bool),
    filter: (Filter, bool),
    selected_match_guid: Option<Uuid>,
) {
    let player_values = create_player_values(
        session_details,
        main_character,
        statistic,
        filter,
        selected_match_guid,
        colors(ui),
    );

    let Some(max_value) = player_values
        .iter()
        .flat_map(|pv| pv.values.iter().map(|dv| &dv.value))
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
    statistic: (Statistic, bool),
    filter: (Filter, bool),
    selected_match_guid: Option<Uuid>,
    colors: ThemeColors,
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
                existing.values.push(DisplayValue {
                    value: map_statistic(statistic.0, player),
                    color: match filter.0 {
                        Filter::NoFilter => colors.on_panel,
                        Filter::WinLoss => match m.won_by_player_primary_id(&main_character.primary_id) {
                            Some(true) => colors.success,
                            Some(false) => colors.error,
                            None => colors.on_panel,
                        },
                        Filter::MVP => match m.primary_id_was_mvp(&main_character.primary_id) {
                            Some(true) => colors.warning,
                            Some(false) => colors.on_panel,
                            None => colors.on_panel,
                        },
                        Filter::Time => colors.on_panel,
                    },
                    emphasized: Some(m.match_guid) == selected_match_guid,
                });
            } else {
                allies.push(PlayerValues {
                    primary_id: player.primary_id.clone(),
                    display_name: player.display_name.clone(),
                    values: vec![DisplayValue {
                        value: map_statistic(statistic.0, player),
                        color: match filter.0 {
                            Filter::NoFilter => colors.on_panel,
                            Filter::WinLoss => colors.accent,
                            Filter::MVP => colors.warning,
                            Filter::Time => colors.on_panel,
                        },
                        emphasized: Some(m.match_guid) == selected_match_guid,
                    }],
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
            let value = enemy_team
                .players
                .iter()
                .map(|p| map_statistic(statistic.0, p))
                .sum::<f32>()
                / enemy_team.players.len() as f32;
            Some(DisplayValue {
                value,
                color: match filter.0 {
                    Filter::NoFilter => colors.on_panel,
                    Filter::WinLoss => colors.accent,
                    Filter::MVP => colors.warning,
                    Filter::Time => colors.on_panel,
                },
                emphasized: Some(m.match_guid) == selected_match_guid,
            })
        })
        .collect();

    result.push(PlayerValues {
        primary_id: String::new(),
        display_name: "Average Enemy".into(),
        values: enemy_values,
    });

    result
}

fn map_statistic(statistic: Statistic, p: &full::Player) -> f32 {
    match statistic {
        Statistic::Score => p.score as f32,
        Statistic::Goals => p.goals as f32,
        Statistic::Shots => p.shots as f32,
        Statistic::Assists => p.assists as f32,
        Statistic::Saves => p.saves as f32,
        Statistic::Touches => p.touches as f32,
        Statistic::CarTouches => p.car_touches as f32,
        Statistic::Demos => p.demos as f32,
        Statistic::PercentBoosting => p
            .player_stats
            .as_ref()
            .map_or(0.0, |ps| ps.percent_boosting) as f32,
        Statistic::PercentDemolished => p
            .player_stats
            .as_ref()
            .map_or(0.0, |ps| ps.percent_demolished) as f32,
        Statistic::PercentOnGround => p
            .player_stats
            .as_ref()
            .map_or(0.0, |ps| ps.percent_on_ground) as f32,
        Statistic::PercentOnWall => {
            p.player_stats.as_ref().map_or(0.0, |ps| ps.percent_on_wall) as f32
        }
        Statistic::PercentPowersliding => {
            p.player_stats
                .as_ref()
                .map_or(0.0, |ps| ps.percent_powersliding) as f32
        }
        Statistic::PercentSupersonic => p
            .player_stats
            .as_ref()
            .map_or(0.0, |ps| ps.percent_supersonic) as f32,
    }
}
