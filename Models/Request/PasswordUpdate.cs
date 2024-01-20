namespace tsuyu.Models;

public record PasswordUpdate {
	public string Password { get; set; }
	public string NewPassword { get; set; }
}
