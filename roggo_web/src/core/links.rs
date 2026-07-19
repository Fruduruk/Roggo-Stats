pub fn to_tracker_network_link(primary_id: &str, username: &str) -> Option<String> {
    let parts: Vec<_> = primary_id.split("|").collect();
    let platform = parts[0];
    let id = parts[1];

    match platform {
        "Epic" => Some(format!("https://rocketleague.tracker.network/rocket-league/profile/epic/{username}/overview")),
        "PS4" => Some(format!("https://rocketleague.tracker.network/rocket-league/profile/psn/{username}/overview")),
        "Steam" => Some(format!("https://rocketleague.tracker.network/rocket-league/profile/steam/{id}/overview")),
        "XboxOne" => Some(format!("https://rocketleague.tracker.network/rocket-league/profile/xbl/{username}/overview")),
        _ => None
    }
}