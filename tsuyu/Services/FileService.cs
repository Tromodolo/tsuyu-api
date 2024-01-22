using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace tsuyu.Services; 

public class FileService : IFileService {
	string FileStoragePath;

	public FileService() {
		FileStoragePath = Path.Combine(
			Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "files");
	}

	public string CalculateHash(Stream stream) {
		using var md5 = MD5.Create();
		var hash = md5.ComputeHash(stream);

		// Reset stream position after computation, because ComputeHash sets it
		stream.Position = 0;

		return Convert.ToBase64String(hash);
	}

	public string GetRandomizedString(uint length) {
		var possibleCharacters =
			"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_-";
		var builder = new StringBuilder();

		for (int i = 0; i < length; i++) {
			builder.Append(
				possibleCharacters[Random.Shared.Next(0, 64)]);
		}

		return builder.ToString();
	}

	public async Task StoreFileAsync(Stream stream, string fileName) {
		var filePath = Path.Combine(FileStoragePath, fileName);
		await using var fileStream = new FileStream(filePath, FileMode.CreateNew);
		await stream.CopyToAsync(fileStream);
	}

	public void DeleteFileAsync(string fileName) {
		var filePath = Path.Combine(FileStoragePath, fileName);
		// TODO: Possibly find a more asynchronous way of doing this
		if (File.Exists(filePath)) {
			File.Delete(filePath);
		}
	}
}