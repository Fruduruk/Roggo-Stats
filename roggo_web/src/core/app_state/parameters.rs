use uuid::Uuid;

pub struct Parameters {
    pub date: jiff::civil::Date,
    pub session_match_list: Vec<Uuid>,
}

impl Default for Parameters {
    fn default() -> Self {
        Self {
            date: jiff::Zoned::now().date(),
            session_match_list: vec![],
        }
    }
}
