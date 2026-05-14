use gloo_net::http::Request;

use crate::core::dto::{AgentErrorDto, MainCharacterDto, MatchDto};
use crate::core::{Error, Result};

const WEB_SOCKET_ADDR: &str = "http://127.0.0.1:49124";

pub async fn get_main_character() -> Result<String> {
    let response = Request::get(&format!("{WEB_SOCKET_ADDR}/main_character"))
        .send()
        .await?;

    if response.ok() {
        let dto = response.json::<MainCharacterDto>().await?;

        Ok(dto.username)
    } else {
        let error_dto = response.json::<AgentErrorDto>().await?;

        Err(Error::AgentError(error_dto))
    }
}
