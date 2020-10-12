use warp::{Rejection, Reply, Filter};
use std::convert::Infallible;
use warp::http::StatusCode;
use std::{error, fs, io::ErrorKind};
use sqlx::{MySql, Pool};

mod account;
mod rejections;
mod db;
mod routes;

#[tokio::main]
async fn main() -> Result<(), Box<dyn error::Error>> {
    if let Err(e) = fs::create_dir("./files") {
        match e.kind() {
            ErrorKind::AlreadyExists => (),
            _ => panic!("Failed to create files folder"),
        }
    }

    let db: Pool<MySql> = db::initialize_db_pool().await.expect("Failed to initialize database connection");
    db::create_tables(&db).await;

    let routes = routes::get_routes(&db);

    /* Router Setup */
    let router = routes.recover(handle_error);
    println!("Server started at localhost:8080");
    warp::serve(router).run(([0, 0, 0, 0], 8080)).await;

    Ok(())
}

async fn handle_error(err: Rejection) -> Result<impl Reply, Infallible> {
    let (code, message) =
        if err.is_not_found() {
            (StatusCode::NOT_FOUND, String::from("File not found"))
        }
        else if err.find::<warp::reject::PayloadTooLarge>().is_some() {
            (StatusCode::BAD_REQUEST, String::from("Payload too large"))
        }
        else if err.find::<rejections::Unauthorized>().is_some() {
            (StatusCode::UNAUTHORIZED, String::from("Not allowed to upload files"))
        }
        else if err.find::<rejections::BadRequest>().is_some() {
            (StatusCode::BAD_REQUEST, String::from("There is a problem with uploading requested data. Please try again"))
        }
        else if err.find::<rejections::Banned>().is_some() {
            (StatusCode::FORBIDDEN, String::from("Your IP has been banned from uploading files"))
        }
        else {
            eprintln!("Unhandled error: {:?}", err);
            (StatusCode::INTERNAL_SERVER_ERROR, String::from("Internal server error"))
        };

    return Ok(warp::reply::with_status(message, code));
}

