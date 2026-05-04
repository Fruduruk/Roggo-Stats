use std::{
    fs::{self, File},
    io::{self, Write},
    path::PathBuf,
};
pub struct PacketCollector {
    dir: PathBuf,
}

impl PacketCollector {
    pub fn new(dir: impl Into<PathBuf>) -> io::Result<Self> {
        let dir = dir.into();
        fs::create_dir_all(&dir)?;

        Ok(Self { dir })
    }

    pub fn next(&mut self, timestamp: u128, raw: &str) {
        let file_name = format!("{}.json", timestamp);
        let path = self.dir.join(file_name);

        let mut file = File::create(path).unwrap();
        file.write_all(raw.as_bytes()).unwrap();
    }
}
