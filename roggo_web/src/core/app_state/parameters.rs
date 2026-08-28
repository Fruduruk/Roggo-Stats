pub struct Parameters {
    pub date: jiff::civil::Date,
}

impl Default for Parameters {
    fn default() -> Self {
        Self {
            date: jiff::Zoned::now().date(),
        }
    }
}
