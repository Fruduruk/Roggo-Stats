use std::sync::{Arc, Mutex};
use crate::core::{Error, Result};

use crate::core::{api, ui::app::Content};

pub fn load_main_character(state: Arc<Mutex<Content>>) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_main_character().await;

        {
            let Ok(mut state) = state.lock() else {
                return;
            };

            match result {
                Ok(name) => {
                    state.player_name = Some(name);
                }
                Err(err) => {
                    match err {
                        Error::HTTPError(_) => {},
                        Error::AgentError(agent_error_dto) => state.current_error = Some(agent_error_dto),
                    }
                }
            }

        }
    });
}