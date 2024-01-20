using Microsoft.AspNetCore.Mvc;

namespace tsuyu.Controllers;

[ApiController]
public class SettingsController : BaseController {
	readonly ConfigurationService Config;

	public SettingsController(Database db, ConfigurationService config) : base(db) {
		Config = config;
	}

	/// <summary>
	/// Used by the frontend to figure out what is allowed and what is not
	/// </summary>
	/// <returns>Response object with the current settings</returns>
	[HttpGet]
	[Route("settings")]
	public IActionResult GetSettingsAsync() {
		return Ok(new Response<CurrentSettings> {
			Data = new CurrentSettings {
				MaxFileSizeBytes = Config.MaxFileSizeBytes,
				RegisterEnabled = Config.RegisterEnabled
			}
		});
	}
}
