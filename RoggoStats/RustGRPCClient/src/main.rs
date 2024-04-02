use ballchasing::ballchasing_client;
use crate::ballchasing::ballchasing_client::BallchasingClient;
use crate::ballchasing::{GroupType, Identity, IdentityType, MatchType, Playlist, FilterRequest};

pub mod ballchasing {
    tonic::include_proto!("ballchasing");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("This is my Greetings gRPC test.");

    let mut client = BallchasingClient::connect("http://192.168.178.30:9000").await?;

    let request = tonic::Request::new(FilterRequest {
        replay_cap: Some(20),
        identities: vec![
            Identity {
                identity_type: IdentityType::Name.into(),
                name_or_id: "Fruduruk".into(),
            }
        ],
        title: None,
        playlist: Playlist::Doubles.into(),
        match_type: MatchType::Ranked.into(),
        season: None,
        min_date: None,
        max_date: None,
        group_type: GroupType::Together.into(),
        min_rank: None,
        max_rank: None,
    });


    let response = client.get_advanced_replays(request).await?;
    let players = response;

    println!("{:?}", &players);

    Ok(())
}
