use eframe::egui;
use futures_util::StreamExt;
use gloo_net::websocket::{Message, futures::WebSocket};
use std::sync::{Arc, Mutex};

const LOCAL_WS_ADDR: &str = "ws://127.0.0.1:49124";

struct App {
    latest_event: Arc<Mutex<String>>,
    latest_json: Arc<Mutex<String>>,
}

impl App {
    fn new(cc: &eframe::CreationContext<'_>) -> Self {
        let latest_event = Arc::new(Mutex::new("Noch keine Daten".to_owned()));
        let latest_json = Arc::new(Mutex::new(String::new()));

        start_websocket_reader(
            latest_event.clone(),
            latest_json.clone(),
            cc.egui_ctx.clone(),
        );

        Self {
            latest_event,
            latest_json,
        }
    }
}

impl eframe::App for App {
    fn ui(&mut self, ui: &mut egui::Ui, _frame: &mut eframe::Frame) {
        egui::CentralPanel::default().show_inside(ui, |ui| {
            ui.heading("Rocket League API Monitor");

            let event = self.latest_event.lock().unwrap().clone();
            let json = self.latest_json.lock().unwrap().clone();

            ui.label(format!("Event: {event}"));

            ui.separator();

            egui::ScrollArea::vertical().show(ui, |ui| {
                ui.monospace(json);
            });
        });
    }
}
fn start_websocket_reader(
    latest_event: Arc<Mutex<String>>,
    latest_json: Arc<Mutex<String>>,
    ctx: egui::Context,
) {
    wasm_bindgen_futures::spawn_local(async move {
        let mut ws = match WebSocket::open(LOCAL_WS_ADDR) {
            Ok(ws) => ws,
            Err(err) => {
                *latest_event.lock().unwrap() = "WebSocket Fehler".to_owned();
                *latest_json.lock().unwrap() = format!("{err:?}");
                ctx.request_repaint();
                return;
            }
        };

        while let Some(message) = ws.next().await {
            let message = match message {
                Ok(Message::Text(text)) => text,
                Ok(Message::Bytes(bytes)) => String::from_utf8_lossy(&bytes).to_string(),
                Err(err) => {
                    *latest_event.lock().unwrap() = "WebSocket Lesefehler".to_owned();
                    *latest_json.lock().unwrap() = format!("{err:?}");
                    ctx.request_repaint();
                    continue;
                }
            };

            let outer: serde_json::Value = match serde_json::from_str(&message) {
                Ok(value) => value,
                Err(err) => {
                    *latest_event.lock().unwrap() = "Ungültiges äußeres JSON".to_owned();
                    *latest_json.lock().unwrap() = format!("{err}\n\n{message}");
                    ctx.request_repaint();
                    continue;
                }
            };

            let event = outer
                .get("Event")
                .and_then(serde_json::Value::as_str)
                .unwrap_or("Unknown")
                .to_owned();

            if event == "UpdateState" {
                if let Some(data_raw) = outer.get("Data").and_then(serde_json::Value::as_str) {
                    match serde_json::from_str::<serde_json::Value>(data_raw) {
                        Ok(data) => {
                            *latest_event.lock().unwrap() = event;
                            *latest_json.lock().unwrap() =
                                serde_json::to_string_pretty(&data).unwrap();
                        }
                        Err(err) => {
                            *latest_event.lock().unwrap() = "Data-JSON Parsefehler".to_owned();
                            *latest_json.lock().unwrap() =
                                format!("{err}\n\nData raw:\n{data_raw}");
                        }
                    }
                }
            } else {
                *latest_event.lock().unwrap() = event;
                *latest_json.lock().unwrap() = serde_json::to_string_pretty(&outer).unwrap();
            }

            ctx.request_repaint();
        }

        *latest_event.lock().unwrap() = "WebSocket geschlossen".to_owned();
        ctx.request_repaint();
    });
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
