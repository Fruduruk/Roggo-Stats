use std::path::PathBuf;

use axum::{Json, Router, extract::State, routing::get};
use tokio::sync::watch;
use tower_http::cors::{Any, CorsLayer};

use crate::core::{
    api::{
        Error, Result,
        dto::{MainCharacterDto, PersonalMatchDto},
    },
    bl::feature,
};

const WEB_SOCKET_ADDR: &str = "127.0.0.1:49124";

#[derive(Clone)]
struct AppState {
    db_file_path: PathBuf,
}

pub async fn run(mut shutdown_rx: watch::Receiver<bool>, db_file_path: PathBuf) -> Result<()> {
    let cors = CorsLayer::new()
        .allow_origin(Any)
        .allow_methods(Any)
        .allow_headers(Any);

    let state = AppState { db_file_path };

    let app = add_routes(Router::new()).layer(cors).with_state(state);

    let listener = tokio::net::TcpListener::bind(WEB_SOCKET_ADDR)
        .await
        .unwrap();

    tracing::info!("Web socket running on http://{}/matches", WEB_SOCKET_ADDR);

    axum::serve(listener, app)
        .with_graceful_shutdown(async move {
            loop {
                if *shutdown_rx.borrow() {
                    break;
                }

                if shutdown_rx.changed().await.is_err() {
                    break;
                }
            }
            tracing::info!("Shutting down web api...");
        })
        .await
        .map_err(|err| Error::AxumError { source: err })?;
    Ok(())
}

fn add_routes(app: Router<AppState>) -> Router<AppState> {
    app.route("/matches", get(get_matches))
        .route("/main_character", get(get_main_character))
}

async fn get_matches(State(state): State<AppState>) -> Result<Json<Vec<PersonalMatchDto>>> {
    let matches = feature::get_all_matches(&state.db_file_path)?;
    tracing::debug!("Requested matches");

    Ok(Json(matches))
}

async fn get_main_character(State(state): State<AppState>) -> Result<Json<MainCharacterDto>> {
    let main_character = feature::get_main_character(&state.db_file_path)?;
    tracing::debug!("Requested main character");
    Ok(Json(main_character))
}
