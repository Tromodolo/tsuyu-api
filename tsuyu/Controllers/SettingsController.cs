using Microsoft.AspNetCore.Mvc;

namespace tsuyu.Controllers;

[ApiController]
public class SettingsController : BaseController {
	readonly IConfigurationService Config;

	public SettingsController(IDatabase db, IConfigurationService config) : base(db) {
		Config = config;
	}

	/// <summary>
	/// Used by the frontend to figure out what is allowed and what is not
	/// </summary>
	/// <returns>Response object with the current settings</returns>
	[HttpGet]
	[Route("settings")]
	public async Task<IActionResult> GetSettingsAsync() {
		var registerEnabled = Config.RegisterEnabled;

		// If no one has registered yet, allow registration of a single user
		var userCount = await Db.GetUserCountAsync();
		if (userCount == 0) {
			registerEnabled = true;
		}

		return Ok(new Response<CurrentSettings> {
			Data = new CurrentSettings {
				MaxFileSizeBytes = Config.MaxFileSizeBytes,
				RegisterEnabled = registerEnabled
			}
		});
	}
}
