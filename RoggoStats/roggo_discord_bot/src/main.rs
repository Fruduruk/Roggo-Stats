use crate::ballchasing::ballchasing_client::BallchasingClient;
use crate::ballchasing::{FilterRequest, GroupType, Identity, IdentityType, MatchType, Playlist};

pub mod ballchasing {
    tonic::include_proto!("ballchasing");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("Started roggo discord bot.");

    let mut client = BallchasingClient::connect("http://192.168.178.30:9000").await?;

    let request = tonic::Request::new(FilterRequest {
        replay_cap: Some(20),
        identities: vec![
            Identity {
                identity_type: IdentityType::Name.into(),
                name_or_id: "Fruduruk".into(),
            }
        ],
        playlist: Playlist::Doubles.into(),
        match_type: MatchType::Ranked.into(),
        title: None,
        season: None,
        min_date: None,
        max_date: None,
        min_rank: None,
        group_type: GroupType::Together.into(),
        max_rank: None,
    });

    let response = client.get_advanced_replays(request).await?;

    let replays = response.into_inner().replays;
    let titles: Vec<String> = replays.into_iter().map(|replay| replay.title).collect();

    println!("{:?}", &titles);

    Ok(())
}