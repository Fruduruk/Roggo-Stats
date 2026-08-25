use futures_channel::mpsc::Sender;

use crate::core::Error;

use crate::core::api;
use crate::core::api_result::APIResult;

pub fn load_version(mut sender: Sender<APIResult>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_version().await;

        match result {
            Ok(version) => {
                // web_sys::console::log_1(&version.clone().into());

                let _ = sender.try_send(APIResult::Version(Some(version)));
            }
            Err(_error) => {
                let _ = sender.try_send(APIResult::Version(None));
            },
        }
    });
}

pub fn load_main_character(mut sender: Sender<APIResult>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_main_character().await;
        match result {
            Ok(name) => {
                let _ = sender.try_send(APIResult::PlayerName(name));
            }
            Err(err) => match err {
                Error::HTTPError(_) => {}
                Error::AgentError(agent_error_dto) => {
                    let _ = sender.try_send(APIResult::AgentError(agent_error_dto));
                }
            },
        }
    });
}

// pub fn load_matches(mut sender: Sender<APIResult>) {
//     wasm_bindgen_futures::spawn_local(async move {
//         let result = api::get_matches().await;

//         if let Ok(mut matches) = result {
//             // matches.sort_by_key(|m| -m.ended_at);
//         }
//     });
// }

// pub fn load_sessions(
//     context: Context,
//     content: Arc<Mutex<match_overview_ui::Content>>,
//     pause_ms: i64,
// ) {
//     wasm_bindgen_futures::spawn_local(async move {
//         let result = api::get_sessions(pause_ms).await;

//         if let Ok(mut content) = content.lock() {
//             if let Ok(mut sessions) = result {
//                 sessions.sort_by_key(|s| -s.ended_at);
//                 content.sessions = Some(sessions);
//             }
//         }
//         context.request_repaint();
//     });
// }

// pub fn load_detailed_match_by_id(
//     context: Context,
//     content: Arc<Mutex<match_ui::Content>>,
//     match_guid: Uuid,
// ) {
//     wasm_bindgen_futures::spawn_local(async move {
//         let result = api::get_match_by_match_guid(match_guid).await;

//         if let Ok(mut content) = content.lock() {
//             if let Ok(detailed_match_dto) = result {
//                 content.detailed_match = Some(detailed_match_dto);
//             }
//         }
//         context.request_repaint();
//     });
// }

// pub fn load_detailed_session(
//     context: Context,
//     content: Arc<Mutex<session_ui::Content>>,
//     match_guids: Vec<Uuid>,
// ) {
//     wasm_bindgen_futures::spawn_local(async move {
//         let result = api::get_session(match_guids).await;

//         if let Ok(mut content) = content.lock() {
//             if let Ok(detailed_session_dto) = result {
//                 content.detailed_session = Some(detailed_session_dto)
//             }
//         }
//         context.request_repaint();
//     });
// }

// pub fn toggle_hide_match(
//     context: Context,
//     match_guid: Uuid,
//     hidden: bool,
//     full_reload_requested: Arc<Mutex<bool>>,
// ) {
//     wasm_bindgen_futures::spawn_local(async move {
//         _ = api::hide_match(match_guid, hidden).await;

//         if let Ok(mut reload) = full_reload_requested.lock() {
//             *reload = true;
//         }
//         context.request_repaint();
//     });
// }
