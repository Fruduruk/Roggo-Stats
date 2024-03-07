use ballchasing::ballchasing_client;
use crate::ballchasing::ballchasing_client::BallchasingClient;
use crate::ballchasing::{GroupType, Identity, IdentityType, MatchType, Playlist, RequestFilter};

pub mod ballchasing {
    tonic::include_proto!("ballchasing");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("This is my Greetings gRPC test.");

    let mut client = BallchasingClient::connect("http://localhost:5007").await?;

    let request = tonic::Request::new(RequestFilter {
        replay_cap: None,
        identities: vec![
            Identity {
                identity_type: IdentityType::Name.into(),
                name_or_id: "Fruduruk".into()
            }
        ],
        title: None,
        playlist: Playlist::All.into(),
        match_type: MatchType::Both.into(),
        season: None,
        min_date: None,
        max_date: None,
        group_type: GroupType::Together.into(),
        min_rank: None,
        max_rank: None,
    });


    let response = client.get_simple_replays(request).await?;


    println!("{:?}", response);

    Ok(())
}
