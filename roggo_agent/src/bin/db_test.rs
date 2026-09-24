use std::path::Path;

use roggo_agent::core::db::Repository;
use uuid::Uuid;

fn main() {
    let repo = Repository::connect(Path::new("test.db")).unwrap();

    let match_guids = vec![
        "9fc700ca11f1b7235ceb499490e3c53a",
        "000456e211f1b7228e7b2e8385745828",
        "ea17d7f611f1b720208c0cad736bcbec",
    ]
    .into_iter()
    .map(|guid| Uuid::parse_str(guid).unwrap())
    .collect::<Vec<_>>();

    let days_played = repo
        .get_start_times_ms( 4)
        .unwrap();

    println!("{:#?}", days_played);
}