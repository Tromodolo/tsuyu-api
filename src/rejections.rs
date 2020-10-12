#[derive(Debug)]
pub struct Unauthorized;
impl warp::reject::Reject for Unauthorized {}

#[derive(Debug)]
pub struct BadRequest;
impl warp::reject::Reject for BadRequest {}

#[derive(Debug)]
pub struct Banned;
impl warp::reject::Reject for Banned {}
