namespace tsuyu.Services;

public interface IDatabase {
	Task<int> GetUserCountAsync();
	/// <summary>
	/// Looks up a User by username and returns it if found.
	/// </summary>
	/// <param name="username">Username of the user to find</param>
	/// <returns>User if it exists, null if not</returns>
	Task<User?> GetUserAsync(string username);
	Task<bool> UserExistsAsync(string username);
	/// <summary>
	/// Looks up a user by API token and returns it if found.
	/// </summary>
	/// <param name="apiToken">Token of the user to find</param>
	/// <returns>User if it exists, null if not</returns>
	Task<User?> GetUserByTokenAsync(string apiToken);
	/// <summary>
	/// Updates API token for specified User Id.
	/// </summary>
	/// <param name="userId">Id of user to update token for</param>
	/// <param name="apiToken">Token to change to</param>
	Task SetApiTokenForUserIdAsync(uint userId, string apiToken);
	/// <summary>
	/// Updates password for specified User Id.
	/// Warning: Password MUST be hashed already with BCrypt
	/// </summary>
	/// <param name="userId">Id of user to update password for</param>
	/// <param name="hashedPassword">(BCrypt) Hash of password to update to</param>
	Task SetNewPasswordForUserIdAsync(uint userId, string hashedPassword);
	Task CreateUserAsync(User user);
	/// <summary>
	/// Stores information about an uploaded file.
	/// </summary>
	/// <param name="uploadedFileMetadata">Metadata of the file to store</param>
	Task CreateFileAsync(UploadedFile uploadedFileMetadata);
	Task<UploadedFile[]> ListFilesAsync(uint userId, string? cursor, uint pageSize);
	Task<UploadedFile?> GetFileByIdAsync(uint fileId);
	Task DeleteFileAsync(uint fileId);
}
