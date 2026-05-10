use std::path::PathBuf;

use axum::{
    Json, Router,
    routing::{get, post},
};
use tokio::sync::watch;
use tower_http::cors::{Any, CorsLayer};
use uuid::Uuid;

use crate::core::{
    api::{Result, dto::MatchDto},
    db::repository::Repository,
};

struct State(PathBuf);

pub async fn run(mut shutdown_rx: watch::Receiver<bool>, db_file_path: PathBuf) -> Result<()> {
    let cors = CorsLayer::new()
        .allow_origin(Any)
        .allow_methods(Any)
        .allow_headers(Any);

    let app = Router::new()
        .route("/match", get(get_match))
        .route("/match", post(post_match))
        .layer(cors);

    let listener = tokio::net::TcpListener::bind("127.0.0.1:3000")
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
        .await?;
    Ok(())
}

async fn get_match() -> Json<MatchDto> {
    Json(MatchDto {
        match_guid: Uuid::new_v4(),
        arena: "DFH Stadium".to_string(),
        duration_seconds: 300,
    })
}

async fn post_match(Json(dto): Json<MatchDto>) -> Json<MatchDto> {
    println!("Received DTO: {dto:#?}");

    Json(dto)
}
