use std::{
    fs::File,
    io::{self, Cursor},
    path::{Path, PathBuf},
};

use sevenz_rust::{SevenZArchiveEntry, SevenZWriter, lzma};

pub struct PacketCollector {
    writer: Option<SevenZWriter<File>>,
}

impl PacketCollector {
    pub fn new(path: impl Into<PathBuf>) -> io::Result<Self> {
        let path = path.into();

        if let Some(parent) = path.parent() {
            std::fs::create_dir_all(parent)?;
        }

        let mut writer = SevenZWriter::create(&path).map_err(to_io_error)?;

        writer.set_content_methods(vec![lzma::LZMA2Options::with_preset(9).into()]);

        Ok(Self {
            writer: Some(writer),
        })
    }

    pub fn next(&mut self, timestamp: i64, raw: &str) -> io::Result<()> {
        let file_name = format!("{timestamp}.json");

        let entry = SevenZArchiveEntry::from_path(Path::new(&file_name), file_name.clone());

        let reader = Cursor::new(raw.as_bytes());

        self.writer
            .as_mut()
            .expect("PacketCollector already finished")
            .push_archive_entry(entry, Some(reader))
            .map_err(to_io_error)?;

        Ok(())
    }
    pub fn finish(mut self) -> io::Result<()> {
        if let Some(writer) = self.writer.take() {
            writer.finish()?;
        }

        Ok(())
    }
}

fn to_io_error(err: sevenz_rust::Error) -> io::Error {
    io::Error::new(io::ErrorKind::Other, err)
}
