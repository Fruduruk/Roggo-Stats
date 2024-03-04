mod client;

use greet::greeter_client::GreeterClient;
use crate::greet::{HelloReply, HelloRequest};

pub mod greet {
    tonic::include_proto!("greet");
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("This is my Greetings gRPC test.");

    let mut client = GreeterClient::connect("http://localhost:5007").await?;

    let request = tonic::Request::new(HelloRequest {
        name: "Fruduruk".into(),
        age: 22
    });


    let response = client.say_hello(request).await?;


    let helloReply = HelloReply{
        message: "nice".into(),
        data: Some(Data{

        })
    }
    println!("{:?}",response);

    Ok(())
}
