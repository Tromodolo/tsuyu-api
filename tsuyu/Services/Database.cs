using Dapper;
using FluentMigrator.Runner;
using MySql.Data.MySqlClient;
using System.Text;

namespace tsuyu.Services;

/// <summary>
/// Handles connection to database (only MySql/MariaDB supported)
/// </summary>
public class Database : IDatabase {
    readonly IConfigurationService ConfigurationService;
    readonly MySqlConnection Connection;

    public Database(IConfigurationService configurationService) {
        ConfigurationService = configurationService;
        Connection = new MySqlConnection(ConfigurationService.DbConnectionString);
    }

    public async Task<int> GetUserCountAsync() {
        var count = await Connection.ExecuteScalarAsync<int>(@"
select count(*) from `users`");
        return count;
    }

    /// <summary>
    /// Looks up a User by username and returns it if found.
    /// </summary>
    /// <param name="username">Username of the user to find</param>
    /// <returns>User if it exists, null if not</returns>
    public async Task<User?> GetUserAsync(string username) {
        var user = await Connection.QuerySingleOrDefaultAsync<User>(@"
select  
    id Id,
    username Username,
    hashed_password HashedPassword,
    email Email,
    is_admin IsAdmin,
    api_key ApiToken,
    created_at CreatedAt
from `users` 
where `username` = @username
",
            new {
                username
            });
        return user;
    }

    public async Task<bool> UserExistsAsync(string username) {
        var exists = await Connection.ExecuteScalarAsync<bool>(@"
select exists (
    select *
    from `users` 
    where `username` = @username
)",
            new {
                username
            });
        return exists;
    }

    /// <summary>
    /// Looks up a user by API token and returns it if found.
    /// </summary>
    /// <param name="apiToken">Token of the user to find</param>
    /// <returns>User if it exists, null if not</returns>
    public async Task<User?> GetUserByTokenAsync(string apiToken) {
        var user = await Connection.QuerySingleOrDefaultAsync<User>(@"
select  
    id Id,
    username Username,
    hashed_password HashedPassword,
    email Email,
    is_admin IsAdmin,
    api_key ApiKey,
    created_at CreatedAt
from `users` 
where `api_key` = @apiToken
",
            new {
                apiToken
            });
        return user;
    }

    /// <summary>
    /// Updates API token for specified User Id.
    /// </summary>
    /// <param name="userId">Id of user to update token for</param>
    /// <param name="apiToken">Token to change to</param>
    public async Task SetApiTokenForUserIdAsync(uint userId, string apiToken) {
        await Connection.ExecuteAsync(@"
update `users` 
set `api_Key`= @apiToken
where id = @userId
",
            new {
                userId,
                apiToken
            });
    }

    /// <summary>
    /// Updates password for specified User Id.
    /// Warning: Password MUST be hashed already with BCrypt
    /// </summary>
    /// <param name="userId">Id of user to update password for</param>
    /// <param name="hashedPassword">(BCrypt) Hash of password to update to</param>
    public async Task SetNewPasswordForUserIdAsync(uint userId, string hashedPassword) {
        await Connection.ExecuteAsync(@"
update `users` 
set `hashed_password`= @hashedPassword 
where id = @userId
",
            new {
                userId,
                hashedPassword
            });
    }

    public async Task CreateUserAsync(User user) {
        await Connection.ExecuteAsync(@"
insert into `users` (
    username,
    hashed_password,
    email,
    is_admin,
    api_key
) values (
    @username,
    @hashedPassword,
    @email,
    @isAdmin,
    @apiToken
)",
            new {
                username = user.Username,
                hashedPassword = user.HashedPassword,
                email = user.Email,
                isAdmin = user.IsAdmin,
                apiToken = user.ApiToken
            });
    }

    /// <summary>
    /// Stores information about an uploaded file.
    /// </summary>
    /// <param name="uploadedFileMetadata">Metadata of the file to store</param>
    public async Task CreateFileAsync(UploadedFile uploadedFileMetadata) {
        await Connection.ExecuteAsync(@"
insert into `files` (
    name, 
    original_name, 
    filetype, 
    file_hash, 
    file_size, 
    uploaded_by, 
    uploaded_by_ip) 
values (
    @name, 
    @origName, 
    @type, 
    @hash, 
    @size, 
    @uploadedBy, 
    @uploadedIp)",
            new {
                name = uploadedFileMetadata.Name,
                origName = uploadedFileMetadata.OriginalName,
                type = uploadedFileMetadata.FileType,
                hash = uploadedFileMetadata.FileHash,
                size = uploadedFileMetadata.FileSizeInKB,
                uploadedBy = uploadedFileMetadata.UploadedBy,
                uploadedIp = uploadedFileMetadata.UploadedByIp
            });
    }

    public async Task<UploadedFile[]> ListFilesAsync(uint userId, string? cursor, uint pageSize) {
        var queryBuilder = new StringBuilder();
        queryBuilder.Append(@"
select 
    id Id,
    name Name,
    original_name OriginalName,
    filetype FileType,
    file_hash FileHash,
    file_size FileSizeInKB,
    uploaded_by UploadedBy,
    uploaded_by_ip UploadedByIp,
    created_at CreatedAt 
from `files` 
where
    `uploaded_by` = @userId
");

        if (!string.IsNullOrEmpty(cursor)) {
            queryBuilder.Append("    and `id` < @cursor");
        }
        queryBuilder.Append(@"
 order by id desc 
 limit @pageSize");
        var query = queryBuilder.ToString();

        var result = await Connection.QueryAsync<UploadedFile>(
            query,
            new { userId, cursor, pageSize });

        return result.ToArray();
    }

    public async Task<UploadedFile?> GetFileByIdAsync(uint fileId) {
        var result = await Connection.QuerySingleOrDefaultAsync<UploadedFile>(@"
select 
    id Id,
    name Name,
    original_name OriginalName,
    filetype FileType,
    file_hash FileHash,
    file_size FileSizeInKB,
    uploaded_by UploadedBy,
    uploaded_by_ip UploadedByIp,
    created_at CreatedAt 
from `files` 
where
    `id` = @fileId
",
            new { fileId });
        return result;
    }

    public async Task DeleteFileAsync(uint fileId) {
        await Connection.ExecuteAsync(@"
delete from `files`
where `id` = @fileId
",
            new { fileId });
    }
}