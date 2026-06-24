pub mod models;

use std::{fs, path::PathBuf};

use crate::{get_app_data_directory, settings::models::AgentConfig};

const CONFIG_FILE_NAME: &str = "config.toml";

pub fn get_config_file_path() -> PathBuf {
    get_app_data_directory().join(CONFIG_FILE_NAME)
}

pub fn load_config_or_default() -> AgentConfig {
    fs::read_to_string(get_config_file_path())
        .ok()
        .and_then(|text| toml::from_str(&text).ok())
        .unwrap_or_default()
}
