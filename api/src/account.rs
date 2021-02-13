use warp::{Rejection, Reply};
use warp::multipart::{FormData, Part};
use futures::TryStreamExt;
use std::env;
use bytes::BufMut;
use serde::{Serialize, Deserialize};
use std::net::{SocketAddr, IpAddr};
use std::hash::{Hash, Hasher};
use std::collections::hash_map::DefaultHasher;
use sqlx::types::chrono;
use rand::Rng;
use rand::distributions::Alphanumeric;
use sqlx::{MySql, Pool};
use bcrypt::{DEFAULT_COST, hash, verify};
use crate::db;
use crate::rejections::{Unauthorized, Banned, BadRequest, NotFound, InternalError, LoginTaken};

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct File {
    pub id: i64,
    pub original_name: String,
    pub name: String,
    pub filetype: String,
	pub file_hash: String,
	pub file_size: u32,
    pub uploaded_by: i64,
    pub uploaded_by_ip: String,
    pub created_at: chrono::DateTime<chrono::Utc>,
}

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

#[derive(Debug, Serialize, Deserialize)]
pub struct UserLogin {
    pub username: String,
    pub password: String,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct UserRegister {
    pub username: String,
    pub password: String,
    pub email: Option<String>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct PasswordUpdate {
    pub password: String,
    pub new_password: String,
}

pub async fn register_user(data: UserRegister, conn: Pool<MySql>) -> Result<impl warp::Reply, warp::reject::Rejection> {
    let email = &data.email.unwrap_or_else(|| String::from(""));
    let check_taken = db::check_if_username_or_email_used(&data.username, email, &conn).await;
    match check_taken {
        Ok(taken) => {
            if taken {
                return Err(warp::reject::custom(LoginTaken));
            }
        },
        Err(err) => {
            eprint!("Error when trying to register a user: {}", err);
            return Err(warp::reject::custom(InternalError))
        }
    }

	let token_gen: String = rand::thread_rng()
        .sample_iter(&Alphanumeric)
        .take(30)
        .map(char::from)
        .collect();

    let hash_password = hash(&data.password, DEFAULT_COST);
    let hashed: String;
    match hash_password {
        Ok(pass) => hashed = pass,
        Err(err) => {
            eprint!("Failed hashing password: {}", err);
            return Err(warp::reject::custom(InternalError));
        }
	}
	
	let num_users = db::get_number_of_users(&conn).await.unwrap();
	let is_admin = if num_users == 0 { true } else { false };

	let user = User {
        id: 0,
        username: String::from(&data.username),
        email: Some(String::from(email)),
        hashed_password: hashed,
        api_key: Some(token_gen),
        is_admin,
        last_update: chrono::Utc::now(),
        created_at: chrono::Utc::now(),
    };
    let create_user = db::create_user(&user, &conn).await;

    if let Err(err) = create_user {
        eprint!("Failed inserting user into database: {}", err);
        return Err(warp::reject::custom(InternalError));
    }

    Ok(warp::reply::json(&user))
}

pub async fn update_user_password(user_id: i64, update_data: PasswordUpdate, conn: Pool<MySql>, requester: User) -> Result<impl warp::Reply, warp::Rejection> {
    if user_id != requester.id && !requester.is_admin {
        return Err(warp::reject::custom(Unauthorized));
    }

    let get_user = db::get_user_by_id(&user_id, &conn).await;
    let user: User;
    match get_user {
        Some(usr) => user = usr,
        None => return Err(warp::reject::custom(InternalError)),
    }

    // // Verify old password
    match verify(&update_data.password, &user.hashed_password) {
         Ok(is_correct) => {
             if !is_correct {
                return Err(warp::reject::custom(Unauthorized));
             }
        },
        Err(err) => {
            eprintln!("Error when trying to verify password when updating {}", err);
            return Err(warp::reject::custom(Unauthorized));
        }
    }

    // Hash new password
    let hash_password = hash(&update_data.new_password, DEFAULT_COST);
    let hashed: String;
    match hash_password {
        Ok(pass) => hashed = pass,
        Err(err) => {
            eprint!("Failed hashing password: {}", err);
            return Err(warp::reject::custom(InternalError));
        }
    }

    let update = db::update_password(&user_id, &hashed, &conn).await;
    if let Err(err) = update {
        eprint!("Failed updating user: {}", err);
        return Err(warp::reject::custom(InternalError));
    }

    Ok("Successfully updated user")
}

pub async fn reset_user_token(user_id: i64, conn: Pool<MySql>, requester: User) -> Result<impl warp::Reply, warp::Rejection> {
    let token_gen: String = rand::thread_rng()
        .sample_iter(&Alphanumeric)
        .take(30)
        .map(char::from)
        .collect();

    if !(user_id == requester.id || requester.is_admin){
        return Err(warp::reject::custom(Unauthorized));
    }

    let update = db::update_user_token(&user_id, &token_gen, &conn).await;
    if let Err(err) = update {
        eprint!("Failed resetting user's token: {}", err);
        return Err(warp::reject::custom(InternalError));
    }

    Ok("Successfully reset token")
}

pub async fn login_user(data: UserLogin, conn: Pool<MySql>) -> Result<impl warp::Reply, warp::reject::Rejection> {
    let get_user = db::check_user_login(&data, &conn).await;
    if let Some(user) = get_user {
        if let Ok(correct) = verify(&data.password, &user.hashed_password) {
            if correct {
                return Ok(warp::reply::json(&user));
            }
        }
    }

    Err(warp::reject::custom(Unauthorized))
}

pub async fn get_user(token: String, conn: Pool<MySql>) -> Result<User, warp::reject::Rejection> {
    let user = db::get_user_by_token(&token, &conn).await;
    if let Some(data) = user {
        return Ok(data);
    }
    Err(warp::reject::custom(Unauthorized))
}


pub async fn delete_file(id: i64, conn: Pool<MySql>, user: User) -> Result<impl Reply, Rejection> {
    let mut can_delete = false;

    let file: File;
    match db::get_file_by_id(&id, &conn).await {
        Some(f) => {
            file = f;
        },
        None => {
            return Err(warp::reject::custom(NotFound));
        }
    }

    if file.uploaded_by == user.id || user.is_admin {
        can_delete = true;
    }

    if can_delete {
        match tokio::fs::remove_file(format!("./files/{}", &file.name)).await {
            Err(e) => {
                eprint!("Failed deleting file from disk: {}", e);
                return Err(warp::reject::custom(InternalError));
            },
            _ => (),
        }
        return match db::delete_file_by_id(&id, &conn).await {
            Ok(_) => Ok("Successfully deleted file"),
            Err(e) => {
                eprint!("Failed deleting file: {}", e);
                Err(warp::reject::custom(InternalError))
            },
        }
    }
    else {
        Err(warp::reject::custom(Unauthorized))
    }
}

pub async fn get_files(page: i64, conn: Pool<MySql>, user: User) -> Result<impl Reply, warp::reject::Rejection> {
    return match db::get_files_for_user(&page, &user, &conn).await {
        Ok(files) => Ok(warp::reply::json(&files)),
        Err(err) => {
            eprint!("Failed fetching file list: {}", err);
            Err(warp::reject::custom(InternalError))
        },
    };
}

pub async fn upload_file(form: FormData, conn: Pool<MySql>, user: User, socket_ip: Option<SocketAddr>) -> Result<impl Reply, Rejection> {
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

            let is_banned = db::is_ip_banned(&ip_string, &conn)
                .await
                .map_err(|e| {
                    eprintln!("Problem when fetching ban status: {}", e);
                    warp::reject::custom(InternalError)
                })?;

            if is_banned == true {
                return Err(warp::reject::custom(Banned));
            }

			let cnf: crate::config::ServerConfig = crate::config::get_server_config().unwrap();
			let rand: String = rand::thread_rng()
				.sample_iter(&Alphanumeric)
				.take(cnf.file_name_length.into())
				.map(char::from)
				.collect();

            let file_extension = get_file_ending(&original_file_name);
			let file_name = format!("{}{}", rand, &file_extension);
			let mut file_size: u32 = 0;

            let value = p
                .stream()
                .try_fold(Vec::new(), |mut vec, data| {
					vec.put(data);
					file_size = (vec.iter().len() / 1000) as u32;
                    async move { Ok(vec) }
                })
                .await
                .map_err(|e| {
                    eprintln!("Reading file error: {}", e);
                    warp::reject::custom(InternalError)
                })?;

            let mut hasher = DefaultHasher::new();
            value.hash(&mut hasher);
            let file_hash = format!("{:X}", hasher.finish()).to_lowercase();

            let existing_file = db::get_existing_file_by_hash(&file_hash, &user.id, &conn).await;
            if let Some(old_file) = existing_file { // If this errored out, it doesn't really do much, so just ignore it
                return Ok(format!("{}/{}", &env::var("BASE_URL").expect("Base URL is not set"), &old_file.name));
            }

            let path = format!("./files/{}", &file_name);
            tokio::fs::write(&path, value).await.map_err(|e| {
                eprint!("Failed writing file: {}", e);
                warp::reject::custom(InternalError)
            })?;

            let file = File {
                id: 0,
                original_name: original_file_name,
                name: file_name,
                filetype: content_type,
                uploaded_by: user.id,
                uploaded_by_ip: ip_string,
				file_hash,
				file_size, // In kb
                created_at: chrono::Utc::now(),
            };

            let res = db::write_file(&file, &conn).await;
            return match res {
                Ok(_) => Ok(format!("{}/{}", &env::var("BASE_URL").expect("Base URL is not set"), &file.name)),
                Err(err) => {
                    eprint!("Failed writing file to database: {}", err);
                    // If it failed to write to db, delete file from disk
                    let remove = tokio::fs::remove_file(&path)
                        .await;
                    if let Err(e) = remove {
                        eprint!("Failed removing file from disk: {}", e);
                        // If we got to this point, something is very wrong, so just go ahead and panic
                        panic!("Failed removing file from disk");
                    }
                    Err(warp::reject::custom(InternalError))
                }
            }
        }
    }
    // Should never hit this point
    return Err(warp::reject::custom(InternalError));
}

fn get_file_ending (name: &str) -> &str {
    let mut found_period = 0;

    let mut index = 0;
    for c in name.chars() {
        if c == '.' {
            found_period = index;
        }
        index += 1;
    }

    return &name[found_period..];
}
