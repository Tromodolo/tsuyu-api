namespace tsuyu.Services; 

/// <summary>
/// Reads configuration from env and exposes it
/// </summary>
public class ConfigurationService : IConfigurationService {
	// Authentication
	public string JwtIssuer { get; }
	public string JwtAudience { get; }
	public string JwtKey { get; }

	// Network
	public string DbConnectionString { get; }
	public string BaseUrl { get; }

	// General Configuration
	public bool RegisterEnabled { get; }
	public ulong MaxFileSizeBytes { get; }
	public uint FileNameLength { get; }

	public ConfigurationService() {
		// Presume these aren't null cause they are checked on startup
		JwtIssuer = Environment.GetEnvironmentVariable("JwtIssuer") ?? string.Empty;
		JwtAudience = Environment.GetEnvironmentVariable("JwtAudience") ?? string.Empty;
		JwtKey = Environment.GetEnvironmentVariable("JwtKey") ?? string.Empty;

		DbConnectionString = Environment.GetEnvironmentVariable("DbConnectionString") ?? string.Empty;
		BaseUrl = Environment.GetEnvironmentVariable("BaseUrl") ?? string.Empty;

		var registerEnabled = Environment.GetEnvironmentVariable("RegisterEnabled") ?? string.Empty;
		if (!bool.TryParse(registerEnabled, out bool enabled)) {
			enabled = true;
		}
		RegisterEnabled = enabled;

		var maxFileSizeBytes = Environment.GetEnvironmentVariable("MaxFileSizeBytes") ?? string.Empty;
		if (!ulong.TryParse(maxFileSizeBytes, out ulong sizeBytes)) {
			sizeBytes = 1024 * 1024 * 100; // 100 MB
		}
		MaxFileSizeBytes = sizeBytes;

		var fileNameLength = Environment.GetEnvironmentVariable("FileNameLength") ?? string.Empty;
		if (!uint.TryParse(fileNameLength, out uint nameLength)) {
			nameLength = 12;
		}
		FileNameLength = nameLength;
	}
}