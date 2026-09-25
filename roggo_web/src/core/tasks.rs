use std::str::FromStr;

use futures_channel::mpsc::Sender;
use jiff::civil::Date;
use uuid::Uuid;

use crate::core::Error;
use crate::core::Result;

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
            }
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
            Err(err) => filter_and_send_agent_error(sender, err),
        }
    });
}

pub fn load_day(mut sender: Sender<APIResult>, date: Date) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_day(date).await;
        match result {
            Ok(day) => {
                let _ = sender.try_send(APIResult::Day(day));
            }
            Err(err) => filter_and_send_agent_error(sender, err),
        }
    });
}

pub fn load_days_played(mut sender: Sender<APIResult>) {
    wasm_bindgen_futures::spawn_local(async move {
        match api::get_days_played().await {
            Ok(days_played) => {
                let result = days_played
                    .days
                    .into_iter()
                    .map(|day| {
                        jiff::civil::Date::from_str(&day)
                            .map_err(|err| Error::GeneralError(err.to_string()))
                    })
                    .collect::<Result<Vec<_>>>()
                    .map(APIResult::DaysPlayed)
                    .unwrap_or_else(APIResult::GeneralError);

                let _ = sender.try_send(result);
            }
            Err(err) => filter_and_send_agent_error(sender, err),
        }
    });
}

fn filter_and_send_agent_error(mut sender: Sender<APIResult>, err: Error) {
    let result = match err {
        Error::HTTPError(_) => None,
        Error::AgentError(agent_error) => Some(APIResult::AgentError(agent_error)),
        err @ Error::GeneralError(_) => Some(APIResult::GeneralError(err)),
    };

    if let Some(result) = result {
        let _ = sender.try_send(result);
    }
}

pub fn load_detailed_session(mut sender: Sender<APIResult>, match_guids: Vec<Uuid>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_session(match_guids).await;

        match result {
            Ok(session) => {
                let _ = sender.try_send(APIResult::DetailedSession(session));
            }
            Err(err) => filter_and_send_agent_error(sender, err),
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
