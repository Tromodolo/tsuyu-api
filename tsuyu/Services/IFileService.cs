namespace tsuyu.Services;

public interface IFileService {
	string CalculateHash(Stream stream);
	string GetRandomizedString(uint length);
	Task StoreFileAsync(Stream stream, string fileName);
	void DeleteFileAsync(string fileName);
}
