use std::path::PathBuf;

use axum::{
    Json, Router,
    extract::State,
    routing::{get, post},
};
use tokio::sync::watch;
use tower_http::cors::{Any, CorsLayer};

use crate::core::{
    api::{Error, Result, dto::MatchDto},
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

    let app = Router::new()
        .route("/match", get(get_match))
        .route("/match", post(post_match))
        .layer(cors)
        .with_state(state);

    let listener = tokio::net::TcpListener::bind(WEB_SOCKET_ADDR)
        .await
        .unwrap();

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

async fn get_match(State(state): State<AppState>) -> Result<Json<Vec<MatchDto>>> {
    let matches = feature::get_all_matches(&state.db_file_path)?;

    Ok(Json(matches))
}

async fn post_match(Json(dto): Json<MatchDto>) -> Result<Json<MatchDto>> {
    println!("Received DTO: {dto:#?}");

    Ok(Json(dto))
}
