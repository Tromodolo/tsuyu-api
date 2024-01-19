use warp::{Filter};
use sqlx::{MySql, Pool};
use std::net::SocketAddr;
use serde::{Serialize, Deserialize};
use crate::account;
use crate::config;


pub fn get_routes (db: &Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    files(db.clone())
        .or(users(db.clone()))
        .or(serve_web())
        .or(settings())
}


// File serving
fn serve_web() -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::fs::dir("public")
}
fn get_files() -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::fs::dir("files")
}

// TODO: Connect this to a config file instead of hardcoding it
fn settings() -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("settings")
        .and(warp::get())
        .and_then(return_settings)
}
async fn return_settings() -> Result<impl warp::Reply, warp::Rejection> {
	let cnf: config::ServerConfig = config::get_server_config().unwrap();

    #[derive(Serialize, Deserialize)]
    struct Settings {
        register_enabled: bool,
        max_file_size_bytes: u64,
    }

    Ok(warp::reply::json(&Settings {
        register_enabled: cnf.register_enabled,
        max_file_size_bytes: cnf.max_file_size_bytes,
    }))
}

fn files(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    get_files()
        .or(lookup_files(db.clone()))
		.or(get_file_count(db.clone()))
        .or(upload_file(db.clone()))
        .or(delete_file(db.clone()))
}
fn upload_file(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
	let cnf: config::ServerConfig = config::get_server_config().unwrap();

    warp::path("upload")
        .and(warp::post())
        .and(warp::multipart::form().max_length(cnf.max_file_size_bytes))
        .and(with_db(db.clone()))
        .and(with_user(db.clone()))
        .and(warp::addr::remote().map(|socket: Option<SocketAddr>| { socket }))
        .and_then(account::upload_file)
}
fn delete_file(db:Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("delete")
        .and(warp::delete())
        .and(warp::path::param()
            .map(|id: i64| { id }))
        .and(with_db(db.clone()))
        .and(with_user(db.clone()))
        .and_then(account::delete_file)
}
fn lookup_files(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("files")
        .and(warp::get())
        .and(warp::path::param()
            .map(|page: i64| { page }))
        .and(with_db(db.clone()))
        .and(with_user(db.clone()))
        .and_then(account::get_files)
}
fn get_file_count(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
	warp::path("file-count")
		.and(warp::get())
		.and(with_db(db.clone()))
		.and(with_user(db.clone()))
		.and_then(account::get_file_count)
}


fn users(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    login_user(db.clone())
        .or(register_user(db.clone()))
        .or(reset_token(db.clone()))
        .or(change_password(db.clone()))
}
fn login_user(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("login")
        .and(warp::post())
        .and(warp::body::content_length_limit(16 * 1024).and(warp::body::json()))
        .and(with_db(db.clone()))
        .and_then(account::login_user)
}
fn register_user(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("register")
        .and(warp::post())
        .and(warp::body::content_length_limit(16 * 1024).and(warp::body::json()))
        .and(with_db(db.clone()))
        .and_then(account::register_user)
}
fn reset_token(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("reset-token")
        .and(warp::post())
        .and(warp::path::param()
            .map(|user_id: i64| { user_id }))
        .and(with_db(db.clone()))
        .and(with_user(db.clone()))
        .and_then(account::reset_user_token)
}
fn change_password(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("change-password")
        .and(warp::post())
        .and(warp::path::param()
            .map(|user_id: i64| { user_id }))
        .and(warp::body::content_length_limit(16 * 1024).and(warp::body::json()))
        .and(with_db(db.clone()))
        .and(with_user(db.clone()))
        .and_then(account::update_user_password)
}


fn with_db(db: Pool<MySql>) -> impl Filter<Extract = (Pool<MySql>,), Error = std::convert::Infallible> + Clone {
    warp::any().map(move || db.clone())
}

fn with_user(db: Pool<MySql>) -> impl Filter<Extract = (account::User,), Error = warp::Rejection> + Clone {
	warp::any().and(warp::header::<String>("Authorization").and(with_db(db.clone())).and_then(account::get_user))
}
