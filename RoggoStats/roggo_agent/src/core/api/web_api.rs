use std::path::PathBuf;

use axum::{
    Json, Router,
    extract::{Path, State},
    routing::{get, post},
};
use tokio::sync::watch;
use tower_http::cors::{Any, CorsLayer};
use uuid::Uuid;

use crate::core::{
    api::{
        Error, Result,
        contract::{
            DetailedMatchDto, DetailedSessionDto, HideRequest, MainCharacterDto, SessionRequest,
            SimpleMatchDto, SimpleSessionDto, VersionDto,
        },
    },
    bl::feature,
    windows_api,
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
        .map_err(|err| Error::ConnectionError(err.to_string()))?;

    // The agent as a tray can start roggo-settings.exe, which changes the config and this restarts the agent.
    // But the agent cannot restart, because the socket is inherited by roggo-settings as a child process, even though it has nothing to do with the web api.
    // This function tells windows, that this socket cannot be inherited.
    windows_api::make_socket_not_inheritable(&listener)
        .map_err(|err| Error::ConnectionError(err.to_string()))?;

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
    app.route("/main_character", get(get_main_character))
        .route("/matches", get(get_matches))
        .route("/matches/{id}", get(get_match_by_id))
        .route("/version", get(get_version))
        .route("/sessions/{pause_ms}", get(get_all_sessions))
        .route("/session", post(get_session))
        .route("/hide_match", post(hide_match))
}

async fn get_matches(State(state): State<AppState>) -> Result<Json<Vec<SimpleMatchDto>>> {
    let matches = feature::get_all_matches(&state.db_file_path)?;
    Ok(Json(matches))
}

async fn get_main_character(State(state): State<AppState>) -> Result<Json<MainCharacterDto>> {
    let main_character = feature::get_main_character(&state.db_file_path)?;
    Ok(Json(main_character))
}

async fn get_match_by_id(
    State(state): State<AppState>,
    Path(match_guid): Path<Uuid>,
) -> Result<Json<DetailedMatchDto>> {
    let dto = feature::get_detailed_match_by_id(&state.db_file_path, match_guid)?;
    Ok(Json(dto))
}

async fn get_version(State(_state): State<AppState>) -> Result<Json<VersionDto>> {
    let dto = feature::get_version();

    Ok(Json(dto))
}

async fn get_all_sessions(
    State(state): State<AppState>,
    Path(pause_ms): Path<i64>,
) -> Result<Json<Vec<SimpleSessionDto>>> {
    let dtos = feature::get_all_sessions(&state.db_file_path, pause_ms)?;

    Ok(Json(dtos))
}

async fn get_session(
    State(state): State<AppState>,
    Json(request): Json<SessionRequest>,
) -> Result<Json<DetailedSessionDto>> {
    if request.match_guids.is_empty() {
        return Err(Error::UserError("Cannot process empty match list".into()));
    }

    let dto = feature::get_detailed_session(&state.db_file_path, request.match_guids)?;

    Ok(Json(dto))
}

async fn hide_match(State(state): State<AppState>, Json(request): Json<HideRequest>) -> Result<()> {
    feature::hide_match(&state.db_file_path, request.match_guid, request.hide)?;
    Ok(())
}
