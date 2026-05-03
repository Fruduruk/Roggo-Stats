#![cfg_attr(target_os = "windows", windows_subsystem = "windows")]

use futures_util::{SinkExt, StreamExt};
use tokio::io::AsyncReadExt;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::watch;
use tokio_tungstenite::tungstenite::Message;

use tray_icon::{
    menu::{Menu, MenuEvent, MenuItem},
    Icon, TrayIcon, TrayIconBuilder,
};

use winit::{
    application::ApplicationHandler,
    event::WindowEvent,
    event_loop::{ActiveEventLoop, EventLoop},
    window::WindowId,
};

const ROCKET_LEAGUE_TCP_ADDR: &str = "127.0.0.1:49123";
const LOCAL_WS_ADDR: &str = "127.0.0.1:9001";
const WEB_UI_URL: &str = "http://127.0.0.1:8080";

fn main() {
    let (shutdown_tx, shutdown_rx) = watch::channel(false);

    std::thread::spawn(move || {
        let runtime = tokio::runtime::Runtime::new().expect("Tokio Runtime konnte nicht starten");

        runtime.block_on(async move {
            if let Err(err) = run_proxy(shutdown_rx).await {
                eprintln!("Proxy Fehler: {err}");
            }
        });
    });

    let event_loop = EventLoop::<UserEvent>::with_user_event()
        .build()
        .expect("EventLoop konnte nicht erstellt werden");

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

enum UserEvent {
    Menu(MenuEvent),
}

struct TrayApp {
    tray_icon: Option<TrayIcon>,
    open_item: Option<MenuItem>,
    quit_item: Option<MenuItem>,
    shutdown_tx: watch::Sender<bool>,
}

impl ApplicationHandler<UserEvent> for TrayApp {
    fn resumed(&mut self, _event_loop: &ActiveEventLoop) {
        if self.tray_icon.is_some() {
            return;
        }

        let open_item = MenuItem::new("Web UI öffnen", true, None);
        let quit_item = MenuItem::new("Beenden", true, None);

        let menu = Menu::new();
        menu.append(&open_item).unwrap();
        menu.append(&quit_item).unwrap();

        let icon = create_icon();

        let tray_icon = TrayIconBuilder::new()
            .with_tooltip("Rocket League API Proxy")
            .with_menu(Box::new(menu))
            .with_icon(icon)
            .build()
            .expect("Tray Icon konnte nicht erstellt werden");

        self.open_item = Some(open_item);
        self.quit_item = Some(quit_item);
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
                rgba.extend_from_slice(&[40, 160, 255, 255]);
            } else {
                rgba.extend_from_slice(&[0, 0, 0, 0]);
            }
        }
    }

    Icon::from_rgba(rgba, width as u32, height as u32).unwrap()
}

async fn run_proxy(
    mut shutdown_rx: watch::Receiver<bool>,
) -> Result<(), Box<dyn std::error::Error>> {
    let listener = TcpListener::bind(LOCAL_WS_ADDR).await?;

    loop {
        tokio::select! {
            _ = shutdown_rx.changed() => {
                if *shutdown_rx.borrow() {
                    break;
                }
            }

            accepted = listener.accept() => {
                let (client_stream, _) = accepted?;

                tokio::spawn(async move {
                    if let Err(err) = handle_client(client_stream).await {
                        eprintln!("Client Fehler: {err}");
                    }
                });
            }
        }
    }

    Ok(())
}

async fn handle_client(client_stream: TcpStream) -> Result<(), Box<dyn std::error::Error>> {
    let ws_stream = tokio_tungstenite::accept_async(client_stream).await?;
    let (mut ws_write, _) = ws_stream.split();

    let mut rl_stream = TcpStream::connect(ROCKET_LEAGUE_TCP_ADDR).await?;

    let mut buffer = [0u8; 8192];

    loop {
        let n = rl_stream.read(&mut buffer).await?;

        if n == 0 {
            break;
        }

        let raw = String::from_utf8_lossy(&buffer[..n]).to_string();
        ws_write.send(Message::Text(raw.into())).await?;
    }

    Ok(())
}