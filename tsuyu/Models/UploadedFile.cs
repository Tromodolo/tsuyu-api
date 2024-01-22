namespace tsuyu.Models; 

public class UploadedFile {
	public uint? Id { get; set; }
	public string OriginalName { get; set; }
	public string Name { get; set; }
	public string FileType { get; set; }
	public string FileHash { get; set; }
	public ulong FileSizeInKB { get; set; }
	public uint UploadedBy { get; set; }
	public string UploadedByIp { get; set; }
	public DateTime CreatedAt { get; set; }

	public override bool Equals(object? other) {
		var otherFile = other as UploadedFile;
		if (otherFile == null) {
			return false;
		}

		return Id.Equals(otherFile.Id) &&
		       OriginalName.Equals(otherFile.OriginalName) &&
		       Name.Equals(otherFile.Name) &&
		       FileType.Equals(otherFile.FileType) &&
		       FileHash.Equals(otherFile.FileHash) &&
		       FileSizeInKB.Equals(otherFile.FileSizeInKB) &&
		       UploadedBy.Equals(otherFile.UploadedBy) &&
		       UploadedByIp.Equals(otherFile.UploadedByIp) &&
		       CreatedAt.Equals(otherFile.CreatedAt);
	}
}