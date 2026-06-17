pub mod core;
pub mod runtimes;

use std::path::PathBuf;

const APP_DIR_NAME: &str = "RoggoStats";
const DB_FILE_NAME: &str = "roggo-agent.db";
pub const AGENT_VERSION: &str = "0.4.0";


pub fn get_app_data_directory() -> PathBuf {
    dirs::data_local_dir()
        .expect("Could not find local app data directory")
        .join(APP_DIR_NAME)
}

pub fn get_db_file_path() -> PathBuf {
    get_app_data_directory().join(DB_FILE_NAME)
}