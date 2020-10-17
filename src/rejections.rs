#[derive(Debug)]
pub struct Unauthorized;
impl warp::reject::Reject for Unauthorized {}

#[derive(Debug)]
pub struct NotFound;
impl warp::reject::Reject for NotFound {}

#[derive(Debug)]
pub struct InternalError;
impl warp::reject::Reject for InternalError {}

#[derive(Debug)]
pub struct BadRequest;
impl warp::reject::Reject for BadRequest {}

#[derive(Debug)]
pub struct LoginTaken;
impl warp::reject::Reject for LoginTaken {}

#[derive(Debug)]
pub struct Banned;
impl warp::reject::Reject for Banned {}
