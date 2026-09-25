use std::path::Path;

use crate::core::api::contract::session::{SessionDto, SessionMatchDto};
use crate::core::api::contract::{MVPType, PlayerDto};
use crate::core::bl::Result;
use crate::core::bl::features::get_most_played_player;
use crate::core::db::Repository;
use uuid::Uuid;

pub fn get(path: &Path, match_guids: Vec<Uuid>) -> Result<SessionDto> {
    let repo = Repository::connect(path)?;

    let main_character = get_most_played_player(&repo)?;

    let enemies = repo.get_session_enemies(match_guids.clone(), main_character.id)?;
    let allies = repo.get_session_allies(match_guids.clone(), main_character.id)?;

    let matches = repo
        .get_session_matches(match_guids, main_character.id)?
        .into_iter()
        .map(|row| {
            let mvp_type = if row.own_best_global_player_id == main_character.id {
                if row.main_character_won.unwrap_or(true) {
                    MVPType::MVP
                } else {
                    MVPType::ACE
                }
            } else {
                MVPType::Nothing
            };

            let match_enemies = enemies
                .iter()
                .filter(|enemy| enemy.match_guid == row.match_guid)
                .map(|enemy| PlayerDto {
                    primary_id: enemy.primary_id.clone(),
                    display_name: enemy.display_name.clone(),
                })
                .collect();

            let match_allies = allies
                .iter()
                .filter(|ally| ally.match_guid == row.match_guid)
                .map(|ally| PlayerDto {
                    primary_id: ally.primary_id.clone(),
                    display_name: ally.display_name.clone(),
                })
                .collect();

            SessionMatchDto {
                match_guid: row.match_guid,
                created_at: row.created_at,
                ended_at: row.ended_at,
                won: row.main_character_won,
                mvp_type,
                overtime: row.had_overtime,
                arena: row.arena,
                own_score: row.own_score,
                enemy_score: row.enemy_score,
                enemies: match_enemies,
                deleted: row.deleted,
                duration: row.duration,
                allies: match_allies,
            }
        })
        .collect();

    Ok(SessionDto { matches })
}
