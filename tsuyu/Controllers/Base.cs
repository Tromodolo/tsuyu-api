using Microsoft.AspNetCore.Mvc;

namespace tsuyu.Controllers;

public class BaseController : ControllerBase {
	protected readonly IDatabase Db;

	public BaseController(IDatabase db) {
		Db = db;
	}

	/// <summary>
	/// Reads out the user that was authenticated from the Bearer header.
	/// This is a consequence of having permanent tokens...
	/// </summary>
	/// <returns>Authenticated user if found with token</returns>
	protected async Task<User> GetAuthenticatedUserAsync(string token) {
		ArgumentNullException.ThrowIfNull(token); // Shouldn't happen

		// Removing "Bearer " from start of header
		token = token.Substring(token.IndexOf(" ") + 1);

		var user = await Db.GetUserByTokenAsync(token);
		ArgumentNullException.ThrowIfNull(user); // Shouldn't happen
		return user;
	}
}
