use axum::{
    Json, Router,
    routing::{get, post},
};
use serde::{Deserialize, Serialize};
use tower_http::cors::{Any, CorsLayer};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
struct MatchDto {
    match_guid: Uuid,
    arena: String,
    duration_seconds: i64,
}

#[tokio::main]
async fn main() {
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

    axum::serve(listener, app).await.unwrap();
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
