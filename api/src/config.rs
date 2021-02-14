use confy;
use anyhow;
use serde::{Serialize, Deserialize};

#[derive(Serialize, Deserialize)]
pub struct ServerConfig {
	pub port: u16,
	pub database_url: String,
	pub register_enabled: bool,
	pub max_file_size_bytes: u64,
	pub file_name_length: u8,
	pub base_url: String,
}

impl ::std::default::Default for ServerConfig {
	fn default() -> Self {
		Self {
			database_url: "".into(),
			max_file_size_bytes: 100_000_000,
			port: 7000,
			register_enabled: true,
			file_name_length: 12,
			base_url: "http://localhost:7000".into(),
		}
	}
}

pub fn get_server_config() -> anyhow::Result<ServerConfig> {
	let cfg: ServerConfig = confy::load_path("./server_config.toml")
						.expect("Please fill out the server config.");
	Ok(cfg)
}