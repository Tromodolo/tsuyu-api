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
}