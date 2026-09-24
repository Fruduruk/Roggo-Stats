use gloo_net::http::Request;
use jiff::civil::Date;
use uuid::Uuid;

use crate::core::{
    Error, Result, contract::{
        AgentErrorDto, PlayerDto, VersionDto, day::{DayDto, DaysPlayedDto}, session::{DetailedSessionDto, SessionDto, SessionRequest},
    },
};

const WEB_SOCKET_ADDR: &str = "http://127.0.0.1:49122";

pub fn request(route: &str) -> gloo_net::http::RequestBuilder {
    Request::get(&format!("{WEB_SOCKET_ADDR}/{route}"))
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
        Ok(response.json::<PlayerDto>().await?.display_name)
    } else {
        parse_error(response).await
    }
}

async fn parse_error<T>(response: gloo_net::http::Response) -> Result<T> {
    let error_dto = response.json::<AgentErrorDto>().await?;

    Err(Error::AgentError(error_dto))
}

pub async fn get_day(date: Date) -> Result<DayDto> {
    let response = request(&format!("day/{}", date)).send().await?;
    if response.ok() {
        Ok(response.json::<DayDto>().await?)
    } else {
        parse_error(response).await
    }
}

pub async fn get_days_played() -> Result<DaysPlayedDto> {
    let response = request("days_played").send().await?;
    if response.ok() {
        Ok(response.json::<DaysPlayedDto>().await?)
    } else {
        parse_error(response).await
    }
}

pub async fn get_session(match_guids: Vec<Uuid>) -> Result<SessionDto> {
    let response = Request::post(&format!("{WEB_SOCKET_ADDR}/session"))
        .json(&SessionRequest { match_guids })?
        .send()
        .await?;

    if response.ok() {
        Ok(response.json::<SessionDto>().await?)
    } else {
        parse_error(response).await
    }
}

// pub async fn hide_match(match_guid: Uuid, hide: bool) -> Result<()> {
//     let response = Request::post(&format!("{WEB_SOCKET_ADDR}/hide_match"))
//         .json(&HideRequest { match_guid, hide })?
//         .send()
//         .await?;

//     if response.ok() {
//         Ok(())
//     } else {
//         parse_error(response).await
//     }
// }

// pub async fn get_sessions(pause_ms: i64) -> Result<Vec<SimpleSessionDto>> {
//     let response = request(&format!("sessions/{pause_ms}")).send().await?;
//     if response.ok() {
//         Ok(response.json::<Vec<SimpleSessionDto>>().await?)
//     } else {
//         parse_error(response).await
//     }
// }

// pub async fn get_matches() -> Result<Vec<SimpleMatchDto>> {
//     let response = request("matches").send().await?;
//     if response.ok() {
//         Ok(response.json::<Vec<SimpleMatchDto>>().await?)
//     } else {
//         parse_error(response).await
//     }
// }

// pub async fn get_match_by_match_guid(match_guid: Uuid) -> Result<DetailedMatchDto> {
//     let response = request(&format!("matches/{}", match_guid)).send().await?;

//     if response.ok() {
//         Ok(response.json::<DetailedMatchDto>().await?)
//     } else {
//         parse_error(response).await
//     }
// }
