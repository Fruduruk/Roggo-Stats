use gloo_net::http::Request;
use uuid::Uuid;

use crate::core::contract::{
    AgentErrorDto, DetailedMatchDto, DetailedSessionDto, MainCharacterDto, SessionRequest, SimpleMatchDto, SimpleSessionDto, VersionDto
};
use crate::core::{Error, Result};

const WEB_SOCKET_ADDR: &str = "http://127.0.0.1:49124";

pub fn request(route: &str) -> gloo_net::http::RequestBuilder {
    Request::get(&format!("{WEB_SOCKET_ADDR}/{route}"))
}

pub async fn get_session(match_guids: Vec<Uuid>) -> Result<DetailedSessionDto> {
    let response = Request::post(&format!("{WEB_SOCKET_ADDR}/session"))
    .json(&SessionRequest {
        match_guids
    })?
    .send()
    .await?;

    if response.ok() {
        Ok(response.json::<DetailedSessionDto>().await?)
    }else {
        parse_error(response).await
    }
}

pub async fn get_sessions(pause_ms: i64) -> Result<Vec<SimpleSessionDto>> {
    let response = request(&format!("sessions/{pause_ms}")).send().await?;
    if response.ok() {
        Ok(response.json::<Vec<SimpleSessionDto>>().await?)
    } else {
        parse_error(response).await
    }
}

pub async fn get_version() -> Result<String> {
    let response = request("version").send().await?;

    if response.ok() {
        Ok(response.json::<VersionDto>().await?.version)
    } else {
        parse_error(response).await
    }
}

pub async fn get_main_character() -> Result<String> {
    let response = request("main_character").send().await?;

    if response.ok() {
        Ok(response.json::<MainCharacterDto>().await?.username)
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
        Ok(response.json::<Vec<SimpleMatchDto>>().await?)
    } else {
        parse_error(response).await
    }
}

pub async fn get_match_by_match_guid(match_guid: Uuid) -> Result<DetailedMatchDto> {
    let response = request(&format!("matches/{}", match_guid)).send().await?;

    if response.ok() {
        Ok(response.json::<DetailedMatchDto>().await?)
    } else {
        parse_error(response).await
    }
}
