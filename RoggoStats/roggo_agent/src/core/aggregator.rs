use serde_json::Value;

pub struct Aggregator {
    count: u64,
}

impl Aggregator {
    pub fn new() -> Self {
        Self { count: 0 }
    }

    pub fn next(&mut self, raw: String) {
        let outer: Value = match serde_json::from_str(&raw) {
            Ok(value) => value,
            Err(err) => {
                println!("Could not read raw packet: {err}");
                println!("{}", raw);
                return;
            }
        };

        let event = outer
            .get("Event")
            .and_then(Value::as_str)
            .unwrap_or("Unknown");

        println!("Received {event}");
        let data_raw = outer.get("Data").and_then(Value::as_str);
        if let Some(data) = data_raw {
            println!("{data}");
        }
    }
}
