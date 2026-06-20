use crate::core::{
    api::contract::{DetailedAverageAdvancedStatsDto, DetailedAverageCoreStatsDto},
};

impl DetailedAverageAdvancedStatsDto {
    pub fn from_options(
        average_percent_boosting: Option<f64>,
        average_percent_demolished: Option<f64>,
        average_percent_on_ground: Option<f64>,
        average_percent_on_wall: Option<f64>,
        average_percent_powersliding: Option<f64>,
        average_percent_supersonic: Option<f64>,
    ) -> Option<Self> {
        Some(Self {
            average_percent_boosting: average_percent_boosting?,
            average_percent_demolished: average_percent_demolished?,
            average_percent_on_ground: average_percent_on_ground?,
            average_percent_on_wall: average_percent_on_wall?,
            average_percent_powersliding: average_percent_powersliding?,
            average_percent_supersonic: average_percent_supersonic?,
        })
    }
}

impl DetailedAverageCoreStatsDto {
    pub fn from_options(
        average_score: Option<f64>,
        average_goals: Option<f64>,
        average_shots: Option<f64>,
        average_shooting_percentage: Option<f64>,
        average_assists: Option<f64>,
        average_saves: Option<f64>,
        average_demos: Option<f64>,
    ) -> Option<Self> {
        Some(Self {
            average_score: average_score?,
            average_goals: average_goals?,
            average_shots: average_shots?,
            average_shooting_percentage,
            average_assists: average_assists?,
            average_saves: average_saves?,
            average_demos: average_demos?,
        })
    }
}
