using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace tsuyu.Controllers;

[ApiController]
[Route("file")]
public class FileController: BaseController {
	readonly IConfigurationService Config;
	readonly IFileService FileService;

	public FileController(IDatabase db, IConfigurationService config, IFileService fileService) : base(db) {
		Config = config;
		FileService = fileService;
	}

	/// <summary>
	/// Uploads a file and stores it to disk where it can be served.
	/// </summary>
	/// <param name="authorization">Token read out from headers</param>
	/// <param name="file">Uploaded file</param>
	/// <returns>URL where file will be available publicly</returns>
	[HttpPost]
	[Route("upload")]
	[Authorize]
	public async Task<IActionResult> UploadAsync([FromHeader] string authorization, [FromForm] IFormFile file) {
		var authenticatedUser = await GetAuthenticatedUserAsync(authorization);

		var fileName = file.FileName;
		var fileExtension = "";

		var extensionIndex = fileName.LastIndexOf('.');
		if (extensionIndex >= 0) {
			fileExtension = fileName.Substring(extensionIndex);
		}

		var remoteIp = HttpContext?.Request?.HttpContext?.Connection?.RemoteIpAddress;

		// File names should be randomized so they aren't easily guessed
		// Too high length can make the url look bad, so default size is 12
		// Example: xaG5vRGKa138.png
		var randomizedString = FileService.GetRandomizedString(Config.FileNameLength);
		var newFileName = randomizedString + fileExtension;

		var fileStream = file.OpenReadStream();
		var fileHash = FileService.CalculateHash(fileStream);

		var fileMetadata = new UploadedFile {
			Name = newFileName,
			OriginalName = fileName,
			FileSizeInKB = (ulong)file.Length / 1024,
			FileType = file.ContentType,
			FileHash = fileHash,
			UploadedBy = authenticatedUser.Id,
			UploadedByIp = remoteIp?.ToString() ?? "-"
		};

		await Db.CreateFileAsync(fileMetadata);
		await FileService.StoreFileAsync(fileStream, newFileName);

		// Don't actually return a Response object this one time
		// Reason being its easier to deal with just a simple url in ShareX
		return Ok($"{Config.BaseUrl}/{fileMetadata.Name}");
	}

	/// <summary>
	/// Lists files in order DESCENDING by upload date.
	/// </summary>
	/// <param name="authorization">Token read out from headers</param>
	/// <param name="cursor">Cursor to start search from. Can be gotten from QueryResponse.Cursor</param>
	/// <param name="pageSize">Number of items to return</param>
	/// <returns>Response object with array of files</returns>
	[HttpGet]
	[Route("list")]
	[Authorize]
	public async Task<IActionResult> ListFilesAsync([FromHeader] string authorization, [FromQuery] string? cursor = null, [FromQuery] uint pageSize = 12) {
		var queryResponse = new QueryResponse<UploadedFile[]>();
		var authenticatedUser = await GetAuthenticatedUserAsync(authorization);

		var files = await Db.ListFilesAsync(authenticatedUser.Id, cursor, pageSize);
		queryResponse.Data = files;
		queryResponse.TotalItems = files.Length;
		queryResponse.Cursor = files
			.LastOrDefault()?
			.Id?
			.ToString();

		return Ok(queryResponse);
	}

	/// <summary>
	/// Deletes file with specified id. Will fail if file is null or if user
	/// didn't upload the file.
	/// </summary>
	/// <param name="authorization">Token read out from headers</param>
	/// <param name="fileId">Id of the file to delete</param>
	/// <returns>Response object with success message</returns>
	[HttpDelete]
	[Route("delete/{fileId}")]
	[Authorize]
	public async Task<IActionResult> DeleteFileAsync([FromHeader] string authorization, [FromRoute] uint fileId) {
		var response = new Response<string>();
		var authenticatedUser = await GetAuthenticatedUserAsync(authorization);

		var file = await Db.GetFileByIdAsync(fileId);
		if (file == null) {
			response.Error = true;
			response.ErrorMessage = "File does not exist.";
			return BadRequest(response);
		}
		if (file.UploadedBy != authenticatedUser.Id) {
			response.Error = true;
			response.ErrorMessage = "No permission to delete file.";
			return Unauthorized(response);
		}
		FileService.DeleteFileAsync(file.Name);
		await Db.DeleteFileAsync(fileId);

		response.Data = "Deleted file successfully.";
		return Ok(response);
	}
}
