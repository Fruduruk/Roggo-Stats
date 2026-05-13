pub mod core;

#[cfg(target_arch = "wasm32")]
fn main() {
    use eframe::wasm_bindgen::JsCast as _;

    wasm_bindgen_futures::spawn_local(async {
        use crate::core::ui::app::RoggoApp;

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
                Box::new(|cc| Ok(Box::new(RoggoApp::new(cc)))),
            )
            .await
            .unwrap();
    });
}

#[cfg(not(target_arch = "wasm32"))]
fn main() {
    panic!("This app is intended for wasm32.");
}
