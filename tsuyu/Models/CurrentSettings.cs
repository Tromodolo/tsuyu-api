namespace tsuyu.Models;

/// <summary>
/// Used for giving the frontend an idea of what settings are currently enabled
/// </summary>
public class CurrentSettings {
	public bool RegisterEnabled { get; set; }
	public ulong MaxFileSizeBytes { get; set; }
}
