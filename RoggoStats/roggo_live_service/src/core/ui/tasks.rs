use eframe::egui::Context;
use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::core::ui::{app, match_overview_ui, match_ui, session_ui};
use crate::core::{Error};

use crate::core::api;

pub fn load_version(context: Context, content: Arc<Mutex<app::Content>>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_version().await;

        if let Ok(mut content) = content.lock() {
            if let Ok(version) = result {
                content.agent_version = Some(version);
            }
        }
        context.request_repaint();
    });
}

pub fn toggle_hide_match(context: Context, match_guid: Uuid, hidden: bool) {
    wasm_bindgen_futures::spawn_local(async move {
        _ = api::hide_match(match_guid, hidden).await;

        context.request_repaint();
    });
}

pub fn load_main_character(context: Context, content: Arc<Mutex<app::Content>>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_main_character().await;

        if let Ok(mut content) = content.lock() {
            match result {
                Ok(name) => {
                    content.player_name = Some(name);
                }
                Err(err) => match err {
                    Error::HTTPError(_) => {}
                    Error::AgentError(agent_error_dto) => {
                        content.current_error = Some(agent_error_dto)
                    }
                },
            }
        }
        context.request_repaint();
    });
}

pub fn load_matches(context: Context, content: Arc<Mutex<match_overview_ui::Content>>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_matches().await;

        if let Ok(mut content) = content.lock() {
            if let Ok(mut matches) = result {
                matches.sort_by_key(|m| -m.ended_at);
                content.matches = Some(matches);
            }
        }
        context.request_repaint();
    });
}

pub fn load_sessions(context: Context, content: Arc<Mutex<match_overview_ui::Content>>,pause_ms: i64) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_sessions(pause_ms).await;

        if let Ok(mut content) = content.lock() {
            if let Ok(mut sessions) = result {
                sessions.sort_by_key(|s| -s.ended_at);
                content.sessions = Some(sessions);
            }
        }
        context.request_repaint();
    });
}

pub fn load_detailed_match_by_id(
    context: Context,
    content: Arc<Mutex<match_ui::Content>>,
    match_guid: Uuid,
) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_match_by_match_guid(match_guid).await;

        if let Ok(mut content) = content.lock() {
            if let Ok(detailed_match_dto) = result {
                content.detailed_match = Some(detailed_match_dto);
            }
        }
        context.request_repaint();
    });
}

pub fn load_detailed_session(
    context: Context,
    content: Arc<Mutex<session_ui::Content>>,
    match_guids: Vec<Uuid>
) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_session(match_guids).await;

        if let Ok(mut content) = content.lock() {
            if let Ok(detailed_session_dto) = result {
                content.detailed_session = Some(detailed_session_dto)
            }
        }
        context.request_repaint();
    });
}