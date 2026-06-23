pub mod core;
pub mod runtimes;

use std::{fs, path::PathBuf};

use serde::{Deserialize, Serialize};

const APP_DIR_NAME: &str = "RoggoStats";
const DB_FILE_NAME: &str = "roggo-agent.db";
const CONFIG_FILE_NAME: &str = "config.toml";

pub const AGENT_VERSION: &str = "0.5.0";

pub fn get_app_data_directory() -> PathBuf {
    dirs::data_local_dir()
        .expect("Could not find local app data directory")
        .join(APP_DIR_NAME)
}

pub fn get_db_file_path() -> PathBuf {
    get_app_data_directory().join(DB_FILE_NAME)
}

pub fn get_config_file_path() -> PathBuf {
    get_app_data_directory().join(CONFIG_FILE_NAME)
}

pub fn load_config_or_default() -> AgentConfig {
    fs::read_to_string(get_config_file_path())
        .ok()
        .and_then(|text| toml::from_str(&text).ok())
        .unwrap_or_default()
}

#[derive(Debug,Clone, Serialize, Deserialize, PartialEq, Eq)]
pub struct AgentConfig {
    pub rl_api_port: u16,
    pub start_ui_when_rl_closes: bool,
}

impl Default for AgentConfig {
    fn default() -> Self {
        Self {
            rl_api_port: 49123,
            start_ui_when_rl_closes: false,
        }
    }
}
