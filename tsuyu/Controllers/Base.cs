using Microsoft.AspNetCore.Mvc;

namespace tsuyu.Controllers;

public class BaseController : ControllerBase {
	protected readonly Database Db;

	public BaseController(Database db) {
		Db = db;
	}

	/// <summary>
	/// Reads out the user that was authenticated from the Bearer header.
	/// </summary>
	/// <remarks>Warning: This should only be used in methods with an [Authorize] attribute,
	/// to ensure that the header always exists.</remarks>
	/// <returns>Authenticated user</returns>
	protected async Task<User> GetAuthenticatedUserAsync() {
		var usedKey = HttpContext.Request.Headers.Authorization.FirstOrDefault();
		ArgumentNullException.ThrowIfNull(usedKey); // Shouldn't happen

		// Removing "Bearer " from start of header
		usedKey = usedKey.Substring(usedKey.IndexOf(" ") + 1);

		var user = await Db.GetUserByTokenAsync(usedKey);
		ArgumentNullException.ThrowIfNull(user); // Shouldn't happen
		return user;
	}
}
