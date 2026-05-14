use gloo_net::http::Request;

use crate::core::contract::{AgentErrorDto, MainCharacterDto, SimpleMatchDto};
use crate::core::{Error, Result};

const WEB_SOCKET_ADDR: &str = "http://127.0.0.1:49124";

pub fn request(route: &str) -> gloo_net::http::RequestBuilder {
    Request::get(&format!("{WEB_SOCKET_ADDR}/{route}"))
}

pub async fn get_main_character() -> Result<String> {
    let response = request("main_character").send().await?;

    if response.ok() {
        let dto = response.json::<MainCharacterDto>().await?;

        Ok(dto.username)
    } else {
        parse_error(response).await
    }
}

async fn parse_error<T>(response: gloo_net::http::Response) -> Result<T> {
    let error_dto = response.json::<AgentErrorDto>().await?;

    Err(Error::AgentError(error_dto))
}

pub async fn get_matches() -> Result<Vec<SimpleMatchDto>> {
    let response = request("matches").send().await?;
    if response.ok() {
        let dto = response.json::<Vec<SimpleMatchDto>>().await?;
        Ok(dto)
    } else {
        parse_error(response).await
    }
}
