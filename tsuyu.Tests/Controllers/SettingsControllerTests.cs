using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using tsuyu.Models;

namespace tsuyu.Tests;

public class SettingsControllerTests {
	SettingsController controller;

	IDatabase db;
	IConfigurationService config;

	Fixture fixture;

	[SetUp]
	public void Setup() {
		config = Substitute.For<IConfigurationService>();
		db = Substitute.For<IDatabase>();

		controller = new SettingsController(db, config);

		fixture = new Fixture();
	}
	
	[Test]
	public async Task TestGetSettingsZeroUsers() {
		var registerEnabled = fixture.Create<bool>();
		var maxFileSizeBytes = fixture.Create<uint>();

		config.RegisterEnabled.Returns(registerEnabled);
		config.MaxFileSizeBytes.Returns(maxFileSizeBytes);

		db.GetUserCountAsync().Returns(Task.FromResult(0));

		var result = await controller.GetSettingsAsync();

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<CurrentSettings>, Is.True);

		var response = (Response<CurrentSettings>)((ObjectResult)result).Value;

		Assert.That(response.Data.RegisterEnabled, Is.True);
		Assert.That(response.Data.MaxFileSizeBytes, Is.EqualTo(maxFileSizeBytes));
		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
	}

	[Test]
	public async Task TestGetSettings() {
		var registerEnabled = fixture.Create<bool>();
		var maxFileSizeBytes = fixture.Create<uint>();
		var userCount = fixture.Create<int>();

		config.RegisterEnabled.Returns(registerEnabled);
		config.MaxFileSizeBytes.Returns(maxFileSizeBytes);

		db.GetUserCountAsync().Returns(Task.FromResult(userCount));

		var result = await controller.GetSettingsAsync();

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<CurrentSettings>, Is.True);

		var response = (Response<CurrentSettings>)((ObjectResult)result).Value;

		Assert.That(response.Data.RegisterEnabled, Is.EqualTo(registerEnabled));
		Assert.That(response.Data.MaxFileSizeBytes, Is.EqualTo(maxFileSizeBytes));
		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
	}
}
