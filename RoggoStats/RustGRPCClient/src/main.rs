use ballchasing::ballchasing_client;
use crate::ballchasing::ballchasing_client::BallchasingClient;
use crate::ballchasing::{Identity, IdentityType, MatchType, Playlist, RequestFilter};

pub mod ballchasing {
    tonic::include_proto!("ballchasing");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("This is my Greetings gRPC test.");

    let mut client = BallchasingClient::connect("http://localhost:5007").await?;

    let request = tonic::Request::new(RequestFilter {
        replay_cap: None,
        filter_name: None,
        identities: vec![
            Identity {
                r#type: IdentityType::Name.into(),
                name_or_id: "Fruduruk".into()
            }
        ],
        title: None,
        playlist: Playlist::All.into(),
        match_type: MatchType::Both.into(),
        season: None,
        min_date: None,
        max_date: None,
    });


    let response = client.get_simple_replays(request).await?;


    println!("{:?}", response);

    Ok(())
}
