using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using tsuyu.Models;

namespace tsuyu.Tests;

public class FileControllerTests {
	FileController controller;

	IDatabase db;
	IFileService fileService;
	IConfigurationService config;

	Fixture fixture;

	[SetUp]
	public void Setup() {
		config = Substitute.For<IConfigurationService>();
		db = Substitute.For<IDatabase>();
		fileService = Substitute.For<IFileService>();

		controller = new FileController(db, config, fileService);

		fixture = new Fixture();
	}

	[Test]
	public async Task TestUploadFile() {
		var token = fixture.Create<string>();

		var fileName = fixture.Create<string>();
		var fileContentLength = fixture.Create<uint>();
		var fileContent = fixture.Create<string>();
		var fileHash = fixture.Create<string>();

		var newFileName = fixture.Create<string>();
		var newFileNameLength = fixture.Create<uint>();

		var baseUrl = fixture.Create<string>();

		var user = fixture.Create<User>();
		db.GetUserByTokenAsync(token).Returns(user);

		fileService.CalculateHash(Stream.Null).Returns(fileHash);
		fileService.GetRandomizedString(newFileNameLength).Returns(newFileName);

		var file = Substitute.For<IFormFile>();
		file.FileName.Returns(fileName);
		file.Length.Returns(fileContentLength);
		file.ContentType.Returns(fileContent);
		file.OpenReadStream().Returns(Stream.Null);

		config.FileNameLength.Returns(newFileNameLength);
		config.BaseUrl.Returns(baseUrl);

		var result = await controller.UploadAsync(token, file);

		// TODO: Uncomment this when CreatedAt is no longer manually set and
		// has turned into a generated field in the db.
		// It currently causes problems cause of equality check
		// db.Received().CreateFileAsync(new UploadedFile {
		// 	Name = newFileName,
		// 	OriginalName = fileName,
		// 	FileSizeInKB = (ulong)fileContentLength / 1024,
		// 	FileType = fileContent,
		// 	FileHash = fileHash,
		// 	CreatedAt = DateTime.UtcNow,
		// 	UploadedBy = user.Id,
		// 	UploadedByIp = "-"
		// });

		fileService.Received().StoreFileAsync(Stream.Null, newFileName);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value, Is.EqualTo($"{baseUrl}/{newFileName}"));
	}

	[Test]
	public async Task TestListFiles() {
		var token = fixture.Create<string>();

		var user = fixture.Create<User>();
		db.GetUserByTokenAsync(token).Returns(user);

		var cursor = fixture.Create<string>();
		var pageSize = fixture.Create<uint>();
		var files = fixture.Create<UploadedFile[]>();

		db.ListFilesAsync(user.Id, cursor, pageSize)
			.Returns(Task.FromResult(files));

		var result = await controller.ListFilesAsync(token, cursor, pageSize);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is QueryResponse<UploadedFile[]>, Is.True);

		var response = (QueryResponse<UploadedFile[]>)((ObjectResult)result).Value;

		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
		Assert.That(response.Data, Is.EqualTo(files));
		Assert.That(response.TotalItems, Is.EqualTo(files.Length));
		Assert.That(response.Cursor, Is.EqualTo(files.Last().Id.ToString()));
	}

	[Test]
	public async Task TestDeleteFileNotExist() {
		var token = fixture.Create<string>();

		var user = fixture.Create<User>();
		db.GetUserByTokenAsync(token).Returns(user);

		var fileId = fixture.Create<uint>();

		UploadedFile? file = null;
		db.GetFileByIdAsync(fileId).Returns(Task.FromResult(file));

		var result = await controller.DeleteFileAsync(token, fileId);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<string>, Is.True);

		var response = (Response<string>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.Null);
		Assert.That(response.Error, Is.True);
		Assert.That(response.ErrorMessage, Is.EqualTo("File does not exist."));
	}

	[Test]
	public async Task TestDeleteFileNotAllowed() {
		var token = fixture.Create<string>();

		var user = fixture.Create<User>();
		db.GetUserByTokenAsync(token).Returns(user);

		var fileId = fixture.Create<uint>();

		// UploadedFile.UploadedBy should never match user
		// since the fixture gives them random values,
		// but we might possibly somehow get the same id?
		var file = fixture.Create<UploadedFile>();
		db.GetFileByIdAsync(fileId).Returns(Task.FromResult(file));

		var result = await controller.DeleteFileAsync(token, fileId);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<string>, Is.True);

		var response = (Response<string>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.Null);
		Assert.That(response.Error, Is.True);
		Assert.That(response.ErrorMessage, Is.EqualTo("No permission to delete file."));
	}

	[Test]
	public async Task TestDeleteFileSuccess() {
		var token = fixture.Create<string>();

		var user = fixture.Create<User>();
		db.GetUserByTokenAsync(token).Returns(user);

		var fileId = fixture.Create<uint>();

		// Making sure the user ids match
		var file = fixture.Create<UploadedFile>();
		file.UploadedBy = user.Id;

		db.GetFileByIdAsync(fileId).Returns(Task.FromResult(file));

		var result = await controller.DeleteFileAsync(token, fileId);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<string>, Is.True);

		var response = (Response<string>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.EqualTo("Deleted file successfully."));
		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
	}
}
