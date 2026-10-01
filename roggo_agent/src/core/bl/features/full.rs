use roggo_contract::full::*;
use std::path::Path;
use uuid::Uuid;

use crate::core::bl::Result;
use crate::core::db::Repository;

use std::collections::HashMap;

pub fn get_match_list_by_ids(path: &Path, match_guids: Vec<Uuid>) -> Result<Vec<RLMatch>> {
    let repo = Repository::connect(path)?;

    let matches = repo.get_match_rows_by_match_guids(match_guids)?;
    let teams = repo.get_team_rows_by_match_ids(matches.iter().map(|m| m.id).collect())?;
    let players = repo.get_player_rows_by_team_ids(teams.iter().map(|t| t.id).collect())?;
    let player_ids = players.iter().map(|p| p.id).collect::<Vec<_>>();
    let player_stats = repo.get_player_stats_rows_by_player_ids(player_ids.clone())?;
    let statfeed_events = repo.get_statfeed_event_rows_by_player_ids(player_ids)?;

    let mut stats_by_player = player_stats
        .into_iter()
        .map(|stats| {
            (
                stats.player_id,
                PlayerStats {
                    percent_boosting: stats.percent_boosting,
                    percent_demolished: stats.percent_demolished,
                    percent_on_ground: stats.percent_on_ground,
                    percent_on_wall: stats.percent_on_wall,
                    percent_powersliding: stats.percent_powersliding,
                    percent_supersonic: stats.percent_supersonic,
                },
            )
        })
        .collect::<HashMap<_, _>>();

    let mut events_by_player = HashMap::<i64, Vec<StatfeedEvent>>::new();

    for event in statfeed_events {
        events_by_player
            .entry(event.main_target_player_id)
            .or_default()
            .push(StatfeedEvent {
                timestamp: event.timestamp,
                event_name: event.event_name,
                event_type: event.event_type,
                secondary_target_primary_id: event.secondary_target_primary_id,
            });
    }

    let mut players_by_team = HashMap::<i64, Vec<Player>>::new();

    for player in players {
        players_by_team
            .entry(player.team_id)
            .or_default()
            .push(Player {
                primary_id: player.primary_id,
                last_username: player.last_username,
                display_name: player.display_name,
                shortcut: player.shortcut,
                score: player.score,
                goals: player.goals,
                shots: player.shots,
                assists: player.assists,
                saves: player.saves,
                touches: player.touches,
                car_touches: player.car_touches,
                demos: player.demos,

                player_stats: stats_by_player.remove(&player.id),

                statfeed_events: events_by_player.remove(&player.id).unwrap_or_default(),
            });
    }

    let mut teams_by_match = HashMap::<i64, Vec<Team>>::new();

    for team in teams {
        teams_by_match.entry(team.match_id).or_default().push(Team {
            team_num: team.team_num,
            name: team.name,
            score: team.score,
            color_primary: team.color_primary,
            color_secondary: team.color_secondary,

            players: players_by_team.remove(&team.id).unwrap_or_default(),
        });
    }

    let matches = matches
        .into_iter()
        .map(|m| RLMatch {
            match_guid: m.match_guid,
            arena: m.arena,
            duration: m.duration,
            created_at: m.created_at,
            ended_at: m.ended_at,
            had_overtime: m.had_overtime,
            deleted: m.deleted,
            playlist: m.playlist,

            teams: teams_by_match.remove(&m.id).unwrap_or_default(),
        })
        .collect();

    Ok(matches)
}
