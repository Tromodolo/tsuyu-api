namespace tsuyu.Services;

public interface IConfigurationService {
	string JwtIssuer { get; }

	string JwtAudience { get; }

	string JwtKey { get; }

	string DbConnectionString { get; }

	string BaseUrl { get; }

	bool RegisterEnabled { get; }

	ulong MaxFileSizeBytes { get; }

	uint FileNameLength { get; }
}
