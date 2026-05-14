use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::core::ui::{app, match_overview_ui, match_ui};
use crate::core::{Error, Result};

use crate::core::{api};

pub fn load_main_character(content: Arc<Mutex<app::Content>>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_main_character().await;

        if let Ok(mut content) = content.lock() {
             match result {
                Ok(name) => {
                    content.player_name = Some(name);
                }
                Err(err) => {
                    match err {
                        Error::HTTPError(_) => {},
                        Error::AgentError(agent_error_dto) => content.current_error = Some(agent_error_dto),
                    }
                }
            }
        }
    });
}

pub fn load_matches(content: Arc<Mutex<match_overview_ui::Content>>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_matches().await;

        if let Ok(mut content) = content.lock() {
            if let Ok(mut matches) = result {
                matches.sort_by_key(|m| -m.ended_at);
                content.matches = Some(matches);
            }
        }
    });
}

pub fn load_detailed_match_by_id(content: Arc<Mutex<match_ui::Content>>, match_guid: Uuid) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_match_by_match_guid(match_guid).await;

        if let Ok(mut content) = content.lock() {
            if let Ok(detailed_match_dto) = result {
                content.detailed_match = Some(detailed_match_dto);
            }
        }
    });
}