use tokio::sync::watch;

use tray_icon::{
    Icon, TrayIcon, TrayIconBuilder,
    menu::{Menu, MenuEvent, MenuItem},
};

use winit::{
    application::ApplicationHandler,
    event::WindowEvent,
    event_loop::{ActiveEventLoop, EventLoop},
    window::WindowId,
};

use crate::core::{agent::run_agent, logging::init_logging};

const WEB_UI_URL: &str = "https://frudd.dev";

pub fn run() {
    let _log_guard = init_logging();

    let (shutdown_tx, shutdown_rx) = watch::channel(false);
    run_agent_thread(shutdown_tx.clone(), shutdown_rx);
    run_tray(shutdown_tx);
}

fn run_agent_thread(shutdown_tx: watch::Sender<bool>, shutdown_rx: watch::Receiver<bool>) {
    std::thread::spawn(move || {
        let runtime = match tokio::runtime::Runtime::new() {
            Ok(runtime) => runtime,
            Err(err) => {
                tracing::error!(error = %err, "Failed to create Tokio runtime");
                return;
            }
        };

        runtime.block_on(async {
            if let Err(err) = run_agent(Some((shutdown_tx, shutdown_rx))).await {
                tracing::error!(error = %err, "Roggo agent failed");
            }
        });
    });
}

enum UserEvent {
    Menu(MenuEvent),
}

struct TrayApp {
    tray_icon: Option<TrayIcon>,
    open_item: Option<MenuItem>,
    quit_item: Option<MenuItem>,
    shutdown_tx: watch::Sender<bool>,
}

fn run_tray(shutdown_tx: watch::Sender<bool>) {
    let event_loop = EventLoop::<UserEvent>::with_user_event()
        .build()
        .expect("Could not create event loop");

    let proxy = event_loop.create_proxy();

    MenuEvent::set_event_handler(Some(move |event| {
        let _ = proxy.send_event(UserEvent::Menu(event));
    }));

    let mut app = TrayApp {
        tray_icon: None,
        open_item: None,
        quit_item: None,
        shutdown_tx,
    };

    event_loop.run_app(&mut app).unwrap();
}

impl ApplicationHandler<UserEvent> for TrayApp {
    fn resumed(&mut self, _event_loop: &ActiveEventLoop) {
        if self.tray_icon.is_some() {
            return;
        }

        let open_item = MenuItem::new("Open Web UI", true, None);
        let exit_item = MenuItem::new("Exit", true, None);

        let menu = Menu::new();
        menu.append(&open_item)
            .expect("Could not append open web ui menu item");
        menu.append(&exit_item)
            .expect("Could not append exit menu item");

        let tray_icon = TrayIconBuilder::new()
            .with_tooltip("Roggo Agent")
            .with_menu(Box::new(menu))
            .with_icon(create_icon())
            .build()
            .expect("Could not create tray icon");

        self.open_item = Some(open_item);
        self.quit_item = Some(exit_item);
        self.tray_icon = Some(tray_icon);
    }

    fn user_event(&mut self, event_loop: &ActiveEventLoop, event: UserEvent) {
        match event {
            UserEvent::Menu(event) => {
                let id = event.id();

                if let Some(open_item) = &self.open_item {
                    if id == open_item.id() {
                        let _ = open::that(WEB_UI_URL);
                    }
                }

                if let Some(quit_item) = &self.quit_item {
                    if id == quit_item.id() {
                        let _ = self.shutdown_tx.send(true);
                        event_loop.exit();
                    }
                }
            }
        }
    }

    fn window_event(
        &mut self,
        _event_loop: &ActiveEventLoop,
        _window_id: WindowId,
        _event: WindowEvent,
    ) {
    }
}

fn create_icon() -> Icon {
    let width = 32;
    let height = 32;

    let mut rgba = Vec::with_capacity(width * height * 4);

    for y in 0..height {
        for x in 0..width {
            let inside = x > 4 && x < 27 && y > 4 && y < 27;

            if inside {
                rgba.extend_from_slice(&[215, 116, 255, 200]);
            } else {
                rgba.extend_from_slice(&[51, 51, 51, 255]);
            }
        }
    }

    Icon::from_rgba(rgba, width as u32, height as u32).unwrap()
}
