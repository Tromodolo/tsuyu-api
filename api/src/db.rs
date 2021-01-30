use sqlx::{MySql, Pool};
use std::{env, error};
use sqlx::mysql::{MySqlPoolOptions};
use crate::account::{UserLogin, User, File, PasswordUpdate};
use futures::TryStreamExt;

#[derive(Debug, sqlx::FromRow)]
struct UserCount {
    num_count: i64,
}

// Should only be used once
pub async fn initialize_db_pool() -> Result<Pool<MySql>, Box<dyn error::Error>> {
    let pool = MySqlPoolOptions::new()
        .max_connections(5)
        .connect(&env::var("DATABASE_URL")?).await?;
    Ok(pool)
}

pub async fn create_tables (pool: &Pool<MySql>){
    sqlx::query("\
        create table if not exists `users` (
            `id` int PRIMARY KEY AUTO_INCREMENT,
            `username` varchar(64) not null,
            `hashed_password` varchar(255) not null,
            `email` varchar(255),
            `is_admin` bool not null,
            `api_key` varchar(128),
            `last_update` datetime not null,
            `created_at` datetime not null
        );"
    ).execute(pool).await.unwrap();

    sqlx::query("\
        create table if not exists `files` (
            `id` int PRIMARY KEY AUTO_INCREMENT,
            `name` varchar(255) not null,
            `original_name` varchar(255) not null,
            `filetype` varchar(64) not null,
            `file_hash` varchar(255) not null,
            `uploaded_by` int not null,
            `uploaded_by_ip` varchar(50) not null,
            `created_at` datetime not null
        );"
    ).execute(pool).await.unwrap();

    sqlx::query("\
        create table if not exists `banned_ips` (
          `id` int PRIMARY KEY AUTO_INCREMENT,
          `ip` varchar(50)
        );"
    ).execute(pool).await.unwrap();

    // Drop and recreate foreign key to make sure that it always exists only once
    sqlx::query("alter table `files` drop foreign key if exists `files_user_id`;")
         .execute(pool).await.unwrap();
    sqlx::query("alter table `files` add foreign key `files_user_id` (`uploaded_by`) references `users` (`id`);")
        .execute(pool).await.unwrap();
}

pub async fn check_user_login (data: &UserLogin, pool: &Pool<MySql>) -> Option<User> {
    let row = sqlx::query_as::<_, User>("select * from `users` where `username` = ?")
        .bind(&data.username)
        .fetch_one(pool)
        .await;

    if let Ok(user) = row {
        return Some(user);
    }
    None
}

pub async fn get_user_by_token(token: &String, pool: &Pool<MySql>) -> Option<User> {
    let row = sqlx::query_as::<_, User>("select * from `users` where `api_key` = ?")
        .bind(token)
        .fetch_one(pool)
        .await;

    if let Ok(user) = row {
        return Some(user);
    }
    None
}

pub async fn get_user_by_id(id: &i64, pool: &Pool<MySql>) -> Option<User> {
    let row = sqlx::query_as::<_, User>("select * from `users` where `id` = ?")
        .bind(id)
        .fetch_one(pool)
        .await;

    if let Ok(user) = row {
        return Some(user);
    }
    None
}

pub async fn get_files_for_user(page: &i64, user: &User, pool: &Pool<MySql>) -> anyhow::Result<Vec<File>> {
    let mut rows = sqlx::query_as::<_, File>("select * from `files` where `uploaded_by` = ? limit 20 offset ?")
        .bind(&user.id)
        .bind((page - 1) * 20)
        .fetch(pool);

    let mut list: Vec<File> = vec![];
    while let Some(file) = rows.try_next().await? {
        list.push(file);
    }
    return Ok(list);
}

pub async fn check_if_username_or_email_used(username: &String, email: &String, pool: &Pool<MySql>) -> anyhow::Result<bool> {
    let mut rows = sqlx::query_as::<_, UserCount>("select COUNT(*) num_count from `users` where username = ? or email = ?")
        .bind(username)
        .bind(email)
        .fetch(pool);

    if let Some(count) = rows.try_next().await? {
        if count.num_count > 0 {
            return Ok(true);
        }
    }
    return Ok(false);
}

pub async fn create_user(user: &User, pool: &Pool<MySql>) -> anyhow::Result<()> {
    sqlx::query("insert into `users` (username, hashed_password, email, is_admin, api_key, last_update, created_at) values (?, ?, ?, ?, ?, ?, ?)")
        .bind(&user.username)
        .bind(&user.hashed_password)
        .bind(&user.email)
        .bind(&user.is_admin)
        .bind(&user.api_key)
        .bind(&user.last_update)
        .bind(&user.created_at)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn update_user_token(user_id: &i64, token: &String, pool: &Pool<MySql>) -> anyhow::Result<()> {
    sqlx::query("update `users` set `api_Key`=? where id = ?")
        .bind(token)
        .bind(user_id)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn update_password(user_id: &i64, new_password: &String, pool: &Pool<MySql>) -> anyhow::Result<()> {
    sqlx::query("update `users` set `hashed_password`=? where id = ?")
        .bind(new_password)
        .bind(user_id)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn write_file(file: &File, pool: &Pool<MySql>) -> anyhow::Result<()> {
    sqlx::query("insert into `files` (name, original_name, filetype, file_hash, uploaded_by, uploaded_by_ip, created_at) values (?, ?, ?, ?, ?, ?, ?)")
        .bind(&file.name)
        .bind(&file.original_name)
        .bind(&file.filetype)
        .bind(&file.file_hash)
        .bind(&file.uploaded_by)
        .bind(&file.uploaded_by_ip)
        .bind(&file.created_at)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn is_ip_banned(ip: &str, pool: &Pool<MySql>) -> anyhow::Result<bool> {
    let mut rows = sqlx::query("select * from `banned_ips` where `ip` = ?")
        .bind(ip)
        .fetch(pool);

    if let Some(_) = rows.try_next().await? {
        return Ok(true);
    }
    return Ok(false);
}

pub async fn get_existing_file_by_hash(hash: &str, user_id: &i64, pool: &Pool<MySql>) -> Option<File> {
    let row = sqlx::query_as::<_, File>("select * from `files` where `uploaded_by` = ? and `file_hash` = ?")
        .bind(user_id)
        .bind(hash)
        .fetch_one(pool)
        .await;

    if let Ok(file) = row {
        return Some(file);
    }
    None
}

pub async fn get_file_by_id(id: &i64, pool: &Pool<MySql>) -> Option<File> {
    let row = sqlx::query_as::<_, File>("select * from `files` where `id` = ?")
        .bind(id)
        .fetch_one(pool)
        .await;

    if let Ok(file) = row {
        return Some(file);
    }
    None
}

pub async fn delete_file_by_id(id: &i64, pool: &Pool<MySql>) -> anyhow::Result<()> {
    sqlx::query("delete from `files` where `id` = ?")
        .bind(id)
        .execute(pool)
        .await?;
    Ok(())
}