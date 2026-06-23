use std::{process::Command, thread::JoinHandle};

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

use crate::{
    core::{agent_supervisor::run_agent, logging::init_logging},
    get_db_file_path,
};

const WEB_UI_URL: &str = "https://roggo.frudd.dev";

pub fn run() {
    let _log_guard = init_logging();

    let instance = single_instance::SingleInstance::new("roggo-stats-agent")
        .expect("Failed to create single instance lock");

    if !instance.is_single() {
        tracing::info!("Roggo Stats Agent is already running. Exiting.");
        return;
    }

    let (shutdown_tx, shutdown_rx) = watch::channel(false);
    let agent_handle = run_agent_thread(shutdown_tx.clone(), shutdown_rx);
    run_tray(shutdown_tx);
    _ = agent_handle.join();
}

fn run_agent_thread(
    shutdown_tx: watch::Sender<bool>,
    shutdown_rx: watch::Receiver<bool>,
) -> JoinHandle<()> {
    std::thread::spawn(move || {
        let runtime = match tokio::runtime::Runtime::new() {
            Ok(runtime) => runtime,
            Err(err) => {
                tracing::error!(error = %err, "Failed to create Tokio runtime");
                return;
            }
        };

        runtime.block_on(async {
            if let Err(err) = run_agent(Some((shutdown_tx, shutdown_rx)), get_db_file_path()).await
            {
                tracing::error!(error = %err, "Roggo agent failed");
                std::process::exit(-1);
            }
        });

        tracing::info!("Roggo Agent shut down gracefully.");
        std::process::exit(0);
    })
}

enum UserEvent {
    Menu(MenuEvent),
}

struct TrayApp {
    tray_icon: Option<TrayIcon>,
    web_item: Option<MenuItem>,
    settings_item: Option<MenuItem>,
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
        web_item: None,
        settings_item: None,
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

        let web_item = MenuItem::new("Web UI (roggo.frudd.dev)", true, None);
        let settings_item = MenuItem::new("Settings", true, None);
        let exit_item = MenuItem::new("Exit", true, None);

        let menu = Menu::new();
        menu.append(&web_item)
            .expect("Could not append web ui menu item");
        menu.append(&settings_item)
            .expect("Could not append settings menu item");
        menu.append(&exit_item)
            .expect("Could not append exit menu item");

        let tray_icon = TrayIconBuilder::new()
            .with_tooltip("Roggo Agent")
            .with_menu(Box::new(menu))
            .with_icon(create_icon())
            .build()
            .expect("Could not create tray icon");

        self.web_item = Some(web_item);
        self.quit_item = Some(exit_item);
        self.settings_item = Some(settings_item);
        self.tray_icon = Some(tray_icon);
    }

    fn user_event(&mut self, event_loop: &ActiveEventLoop, event: UserEvent) {
        match event {
            UserEvent::Menu(event) => {
                let id = event.id();

                if let Some(open_item) = &self.web_item {
                    if id == open_item.id() {
                        let _ = open::that(WEB_UI_URL);
                    }
                }

                if let Some(settings_item) = &self.settings_item {
                    if id == settings_item.id() {
                        let result = Command::new("roggo-settings.exe").spawn();
                        if let Err(err) = result {
                            tracing::error!(error=%err, "Could not run roggo settings");
                        }
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
    let bytes = include_bytes!(concat!(env!("CARGO_MANIFEST_DIR"), "/assets/icon.ico"));

    let image = image::load_from_memory_with_format(bytes, image::ImageFormat::Ico)
        .expect("Could not load tray icon")
        .into_rgba8();

    let (width, height) = image.dimensions();
    let rgba = image.into_raw();

    Icon::from_rgba(rgba, width, height).expect("Could not create tray icon")
}
