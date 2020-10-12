use warp::{Rejection, Reply};
use warp::multipart::{FormData, Part};
use futures::TryStreamExt;
use std::env;
use bytes::BufMut;
use serde::{Serialize, Deserialize};
use uuid::Uuid;
use std::net::{SocketAddr, IpAddr};
use std::hash::{Hash, Hasher};
use std::collections::hash_map::DefaultHasher;
use sqlx::types::chrono;

use crate::db;
use sqlx::{MySql, Pool};
use crate::rejections::{Unauthorized, Banned, BadRequest};

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct User {
    pub id: i64,
    pub username: String,
    pub email: Option<String>,
    pub is_admin: bool,
    pub api_key: Option<String>,
    pub last_update: chrono::DateTime<chrono::Utc>,
    pub created_at: chrono::DateTime<chrono::Utc>,

    #[serde(skip_serializing)]
    pub hashed_password: String,
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct File {
    pub id: i64,
    pub original_name: String,
    pub name: String,
    pub filetype: String,
    pub file_hash: String,
    pub uploaded_by: i64,
    pub uploaded_by_ip: String,
    pub created_at: chrono::DateTime<chrono::Utc>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct UserLoginData {
    pub username: String,
    pub password: String,
}

pub async fn login_user(data: UserLoginData, db: Pool<MySql>) -> Result<impl warp::Reply, warp::reject::Rejection> {
    let user = db::check_user_login(&data, &db).await;
    if let Some(data) = user {
        return Ok(warp::reply::json(&data));
    }
    Err(warp::reject::custom(Unauthorized))
}

pub async fn get_user(token: String, db: Pool<MySql>) -> Result<User, warp::reject::Rejection> {
    let user = db::get_user_by_token(token, &db).await;
    if let Some(data) = user {
        return Ok(data);
    }
    Err(warp::reject::custom(Unauthorized))
}

pub async fn upload_file(form: FormData, db: Pool<MySql>, user: User, socket_ip: Option<SocketAddr>) -> Result<impl Reply, Rejection> {
    let parts: Vec<Part> = form.try_collect().await.map_err(|e| {
        eprintln!("Problem with formdata: {}", e);
        warp::reject::custom(BadRequest)
    })?;

    /* Write file to disk */
    for p in parts {
        if p.name() == "file" {
            let content_type: String;
            if let Some(filetype) = p.content_type() {
                content_type = String::from(filetype);
            }
            else {
                // If it doesn't have a content type, then it might not be a file, so skip
                return Err(warp::reject::custom(BadRequest));
            }

            let original_file_name: String;
            if let Some(name) = p.filename() {
                original_file_name = String::from(name);
            }
            else {
                return Err(warp::reject::custom(BadRequest));
            }

            let mut ip_string: String = String::from("");
            if let Some(socket) = socket_ip {
                let ip = socket.ip();
                match ip {
                    IpAddr::V4(_) => ip_string = format!("{}", ip),
                    IpAddr::V6(_) => ip_string = format!("{}", ip),
                }
            }

            let is_banned = db::is_ip_banned(&ip_string, &db)
                .await
                .map_err(|e| {
                    eprintln!("Problem when fetching ban status: {}", e);
                    warp::reject::reject()
                })?;

            if is_banned == true {
                return Err(warp::reject::custom(Banned));
            }


            let file_extension = get_file_ending(&original_file_name);
            let file_name = format!("{}{}", Uuid::new_v4().to_simple().to_string().to_lowercase(), &file_extension);

            let value = p
                .stream()
                .try_fold(Vec::new(), |mut vec, data| {
                    vec.put(data);
                    async move { Ok(vec) }
                })
                .await
                .map_err(|e| {
                    eprintln!("Reading file error: {}", e);
                    warp::reject::reject()
                })?;

            let mut hasher = DefaultHasher::new();
            value.hash(&mut hasher);
            let file_hash = format!("{:X}", hasher.finish()).to_lowercase();

            let existing_file = db::get_existing_file_by_hash(&file_hash, &user.id, &db).await;
            if let Some(old_file) = existing_file { // If this errored out, it doesn't really do much, so just ignore it
                return Ok(format!("{}/{}", &env::var("BASE_URL").expect("Base URL is not set"), &old_file.name));
            }

            let path = format!("./files/{}", &file_name);
            tokio::fs::write(&path, value).await.map_err(|e| {
                eprint!("Failed writing file: {}", e);
                warp::reject::reject()
            })?;

            let file = File {
                id: 0,
                original_name: original_file_name,
                name: file_name,
                filetype: content_type,
                uploaded_by: user.id,
                uploaded_by_ip: ip_string,
                file_hash,
                created_at: chrono::Utc::now(),
            };

            let res = db::write_file(&file, &db).await;
            return match res {
                Ok(_) => Ok(format!("{}/{}", &env::var("BASE_URL").expect("Base URL is not set"), &file.name)),
                Err(err) => {
                    eprint!("Failed writing file to database: {}", err);
                    // If it failed to write to db, delete file from disk
                    let remove = tokio::fs::remove_file(&path)
                        .await;
                    if let Err(_) = remove {
                        // If we got to this point, something is very wrong, so just go ahead and panic
                       panic!("Failed removing file from disk");
                    }
                    Err(warp::reject::reject())
                }
            }
        }
    }
    // Should never hit this point
    return Err(warp::reject::reject());
}

fn get_file_ending (name: &str) -> &str {
    let mut index = 0;

    for c in name.chars() {
        if c == '.' {
            break;
        }
        index += 1;
    }

    return &name[index..];
}
