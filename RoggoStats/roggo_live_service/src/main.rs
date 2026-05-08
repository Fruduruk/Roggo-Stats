use eframe::egui;
use gloo_net::http::Request;
use serde::{Deserialize, Serialize};
use std::sync::{Arc, Mutex};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
struct MatchDto {
    match_guid: Uuid,
    arena: String,
    duration_seconds: i64,
}

const LOCAL_HTTP_ADDR: &str = "http://127.0.0.1:3000";
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
                    let result = Request::get(&format!("{LOCAL_HTTP_ADDR}/match"))
                        .send()
                        .await;

                    if let Ok(response) = result {
                        if let Ok(dto) = response.json::<MatchDto>().await {
                            *response_text.lock().unwrap() = format!("{dto:#?}");
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
