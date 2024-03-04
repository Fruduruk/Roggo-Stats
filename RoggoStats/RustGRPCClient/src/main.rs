mod client;

use ballchasing_api::basic_api_client;
use crate::ballchasing_api::basic_api_client::BasicApiClient;
use crate::ballchasing_api::SimpleReplayRequest;

pub mod ballchasing_api{
    tonic::include_proto!("ballchasing_api");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("This is my Greetings gRPC test.");

    let mut client = BasicApiClient::connect("http://localhost:5007").await?;

    let request = tonic::Request::new(SimpleReplayRequest {
        id: "Fruduruk".into(),
    });


    let response = client.get_simple_replay(request).await?;



    println!("{:?}",response);

    Ok(())
}
