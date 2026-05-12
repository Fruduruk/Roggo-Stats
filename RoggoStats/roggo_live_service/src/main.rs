pub mod core;

use chrono::{DateTime, Local};
use eframe::egui;
use gloo_net::http::Request;
use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::core::dto::MatchDto;

const WEB_SOCKET_ADDR: &str = "http://127.0.0.1:49124";

struct App {
    response_text: Arc<Mutex<String>>,
}

impl App {
    fn new(_cc: &eframe::CreationContext<'_>) -> Self {
        Self {
            response_text: Arc::new(Mutex::new("Noch keine Antwort.".to_string())),
        }
    }
}

impl eframe::App for App {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.heading("Rocket League API Monitor");

            if ui.button("GET /match").clicked() {
                let response_text = self.response_text.clone();
                let ctx = ui.ctx().clone();

                wasm_bindgen_futures::spawn_local(async move {
                    let result = Request::get(&format!("{WEB_SOCKET_ADDR}/match"))
                        .send()
                        .await;

                    if let Ok(response) = result {
                        if let Ok(dtos) = response.json::<Vec<MatchDto>>().await {
                            let mut text = String::new();

                            for dto in dtos {
                                let date: DateTime<Local> =
                                    DateTime::from_timestamp_millis(dto.created_at)
                                        .unwrap()
                                        .with_timezone(&Local);

                                text.push_str(&format!("{}\n", date.format("%d.%m.%Y %H:%M:%S")));
                            }

                            *response_text.lock().unwrap() = text;
                        }
                    }

                    ctx.request_repaint();
                });
            }

            ui.separator();

            let text = self.response_text.lock().unwrap();
            ui.label(text.as_str());
        });
    }
}

#[cfg(target_arch = "wasm32")]
fn main() {
    use eframe::wasm_bindgen::JsCast as _;

    wasm_bindgen_futures::spawn_local(async {
        let document = web_sys::window().unwrap().document().unwrap();

        let canvas = document
            .get_element_by_id("canvas")
            .unwrap()
            .dyn_into::<web_sys::HtmlCanvasElement>()
            .unwrap();

        eframe::WebRunner::new()
            .start(
                canvas,
                eframe::WebOptions::default(),
                Box::new(|cc| Ok(Box::new(App::new(cc)))),
            )
            .await
            .unwrap();
    });
}

#[cfg(not(target_arch = "wasm32"))]
fn main() {
    panic!("This app is intended for wasm32.");
}
