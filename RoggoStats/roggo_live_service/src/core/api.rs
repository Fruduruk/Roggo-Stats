use gloo_net::http::Request;

use crate::core::dto::{MatchDto, PlayerNameDto};

const WEB_SOCKET_ADDR: &str = "http://127.0.0.1:49124";

pub async fn get_player_name() -> Result<String, String> {
    let response = Request::get(&format!("{WEB_SOCKET_ADDR}/player"))
        .send()
        .await
        .map_err(|err| err.to_string())?;

    let dto = response
        .json::<PlayerNameDto>()
        .await
        .map_err(|err| err.to_string())?;

    Ok(dto.name)
}