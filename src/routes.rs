use warp::{Filter};
use sqlx::{MySql, Pool};
use crate::account;
use std::net::SocketAddr;

// TODO: <Endpoints>
// Files with indexing
// Reset token
// Register user
// Update password
// Getting current server's status for max file upload etc
// TODO: </Endpoints>

pub fn get_routes (db: &Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    files(db.clone())
        .or(users(db.clone()))
}

fn files(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    get_files()
        .or(upload_file(db.clone()))
}

fn get_files() -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::fs::dir("./files")
}

// fn get_file_list() {
//
// }

fn files(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    get_files()
        .or(upload_file(db.clone()))
        .or(delete_file(db.clone()))
}
fn upload_file(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("upload")
        .and(warp::post())
        .and(warp::multipart::form().max_length(5_000_000))
        .and(with_db(db.clone()))
        .and(warp::any().and(warp::header::<String>("authorization").and(with_db(db.clone())).and_then(account::get_user)))
        .and(warp::addr::remote().map(|socket: Option<SocketAddr>| { socket }))
        .and_then(account::upload_file)
}
fn delete_file(db:Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("delete")
        .and(warp::delete())
        .and(warp::path::param()
            .map(|id: i64| { id }))
        .and(with_db(db.clone()))
        .and(warp::any().and(warp::header::<String>("authorization").and(with_db(db.clone())).and_then(account::get_user)))
        .and_then(account::delete_file)
}


fn users(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    login_user(db.clone())
}
fn login_user(db: Pool<MySql>) -> impl Filter<Extract = impl warp::Reply, Error = warp::Rejection> + Clone {
    warp::path("login")
        .and(warp::post())
        .and(warp::body::content_length_limit(16 * 1024).and(warp::body::json()))
        .and(with_db(db.clone()))
        .and_then(account::login_user)
}


fn with_db(db: Pool<MySql>) -> impl Filter<Extract = (Pool<MySql>,), Error = std::convert::Infallible> + Clone {
    warp::any().map(move || db.clone())
}