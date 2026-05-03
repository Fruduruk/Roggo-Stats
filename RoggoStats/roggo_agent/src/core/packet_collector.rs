use std::{
    fs::{self, File},
    io::{self, Write},
    path::PathBuf,
};
pub struct PacketCollector {
    dir: PathBuf,
    count: u64,
}

impl PacketCollector {
    pub fn new(dir: impl Into<PathBuf>) -> io::Result<Self> {
        let dir = dir.into();
        fs::create_dir_all(&dir)?;

        Ok(Self { dir, count: 0 })
    }

    pub fn next(&mut self, raw: &str) {
        self.count += 1;
        let file_name = format!("{:06}.txt", self.count);
        let path = self.dir.join(file_name);
        
        let mut file = File::create(path).unwrap();
        file.write_all(raw.as_bytes()).unwrap();
    }
}
