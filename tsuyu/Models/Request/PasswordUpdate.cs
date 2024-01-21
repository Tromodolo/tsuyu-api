namespace tsuyu.Models;

public record PasswordUpdate {
	/// <summary>
	/// Existing password to verify
	/// </summary>
	public string Password { get; set; }
	/// <summary>
	/// New password to change to
	/// </summary>
	public string NewPassword { get; set; }
}
