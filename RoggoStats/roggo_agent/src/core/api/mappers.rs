use crate::core::api::contract::DetailedAverageAdvancedStatsDto;

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
