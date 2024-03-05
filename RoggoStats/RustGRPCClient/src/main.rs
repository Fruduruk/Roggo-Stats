mod client;

use ballchasing::ballchasing_client;
use crate::ballchasing::ballchasing_client::BallchasingClient;
use crate::ballchasing::RequestFilter;

pub mod ballchasing{
    tonic::include_proto!("ballchasing");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("This is my Greetings gRPC test.");

    let mut client = BallchasingClient::connect("http://localhost:5007").await?;

    let request = tonic::Request::new(RequestFilter {
        replay_cap: None,
        filter_name : None,
        names: vec!["Fruduruk".into()],
        title: None,
        playlist: None,
        free_to_play: None,
        season: None,
        steam_ids: vec![],
        min_date: None,
        max_date: None,
    });


    let response = client.get_simple_replays(request).await?;



    println!("{:?}",response);

    Ok(())
}
