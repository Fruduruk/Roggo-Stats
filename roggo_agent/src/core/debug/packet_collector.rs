use std::{
    fs::File,
    io::{self, Cursor},
    path::{Path, PathBuf},
};

use sevenz_rust::{SeqReader, SevenZArchiveEntry, SevenZWriter, SourceReader, lzma};

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

    pub fn next(&mut self, timestamp: i64, bytes: Vec<u8>) -> io::Result<()> {
        let file_name = format!("{timestamp}.json");

        let entry = SevenZArchiveEntry::from_path(Path::new(&file_name), file_name.clone());

        let reader = Cursor::new(bytes);

        self.writer
            .as_mut()
            .expect("PacketCollector already finished")
            .push_archive_entry(entry, Some(reader))
            .map_err(to_io_error)?;

        Ok(())
    }

    pub fn next_bulk(&mut self, events: Vec<(i64, Vec<u8>)>) -> io::Result<()> {
        if events.is_empty() {
            return Ok(());
        }

        let mut entries = Vec::with_capacity(events.len());
        let mut readers = Vec::with_capacity(events.len());

        for (timestamp, bytes) in events {
            let file_name = format!("{timestamp}.json");

            let mut entry = SevenZArchiveEntry::from_path(Path::new(&file_name), file_name.clone());

            entry.has_stream = true;

            entries.push(entry);
            readers.push(SourceReader::new(Cursor::new(bytes)));
        }

        let reader = SeqReader::new(readers);

        self.writer
            .as_mut()
            .expect("PacketCollector already finished")
            .push_archive_entries(entries, reader)
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
    io::Error::other(err)
}
