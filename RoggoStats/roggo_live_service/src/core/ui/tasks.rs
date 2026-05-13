use std::sync::{Arc, Mutex};

use eframe::egui;

use crate::core::{api, ui::app::Content};

pub fn load_player_name(state: Arc<Mutex<Content>>, ctx: egui::Context) {
    wasm_bindgen_futures::spawn_local(async move {
        let result = api::get_player_name().await;

        {
            let Ok(mut state) = state.lock() else {
                return;
            };

            match result {
                Ok(name) => {
                    state.player_name = Some(name);
                }
                Err(err) => {
                }
            }

        }

        ctx.request_repaint();
    });
}