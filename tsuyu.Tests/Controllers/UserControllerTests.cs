using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using tsuyu.Models;

namespace tsuyu.Tests;

public class UserControllerTests {
	UserController controller;

	IDatabase db;
	IConfigurationService config;

	Fixture fixture;

	[SetUp]
	public void Setup() {
		config = Substitute.For<IConfigurationService>();
		db = Substitute.For<IDatabase>();

		controller = new UserController(db, config);

		fixture = new Fixture();
	}

	[Test]
	public async Task TestLoginInvalid() {
		// Hash: $2y$12$ibm0flWSxXHBgF40vT6mPukO.SzAOAgqK7R7/pkp6T6/7eTGw3ljO
		// Password: "test"
		// Testing with bcrypt in the controller is a bit tricky
		// TODO: Look if its possible to mock bcrypt somehow

		var userLogin = fixture.Create<UserLogin>();

		var user = fixture.Create<User>();
		user.HashedPassword = "$2y$12$ibm0flWSxXHBgF40vT6mPukO.SzAOAgqK7R7/pkp6T6/7eTGw3ljO";

		db.GetUserAsync(userLogin.Username).Returns(Task.FromResult(user));

		var result = await controller.LoginAsync(userLogin);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<User>, Is.True);

		var response = (Response<User>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.Null);
		Assert.That(response.Error, Is.True);
		Assert.That(response.ErrorMessage, Is.EqualTo("Invalid login."));
	}

	[Test]
	public async Task TestLogin() {
		// Hash: $2y$12$ibm0flWSxXHBgF40vT6mPukO.SzAOAgqK7R7/pkp6T6/7eTGw3ljO
		// Password: "test"
		// Testing with bcrypt in the controller is a bit tricky
		// TODO: Look if its possible to mock bcrypt somehow

		var userLogin = fixture.Create<UserLogin>();
		userLogin.Password = "test";

		var user = fixture.Create<User>();
		user.HashedPassword = "$2y$12$ibm0flWSxXHBgF40vT6mPukO.SzAOAgqK7R7/pkp6T6/7eTGw3ljO";

		db.GetUserAsync(userLogin.Username).Returns(Task.FromResult(user));

		var result = await controller.LoginAsync(userLogin);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<User>, Is.True);

		var response = (Response<User>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.EqualTo(user));
		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
	}

	[Test]
	public async Task TestRegisterDisabled() {
		var userRegister = fixture.Create<UserRegister>();
		var userCount = fixture.Create<int>();

		config.RegisterEnabled.Returns(false);

		db.GetUserCountAsync().Returns(Task.FromResult(userCount));

		var result = await controller.RegisterAsync(userRegister);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<User>, Is.True);

		var response = (Response<User>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.Null);
		Assert.That(response.Error, Is.True);
		Assert.That(response.ErrorMessage, Is.EqualTo("Registering is disabled on this instance."));
	}

	[Test]
	public async Task TestRegisterUsernameTaken() {
		var userRegister = fixture.Create<UserRegister>();
		var registerEnabled = fixture.Create<bool>();
		var userCount = fixture.Create<int>();

		config.RegisterEnabled.Returns(registerEnabled);
		db.GetUserCountAsync().Returns(Task.FromResult(userCount));

		db.UserExistsAsync(userRegister.Username).Returns(Task.FromResult(true));

		var result = await controller.RegisterAsync(userRegister);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<User>, Is.True);

		var response = (Response<User>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.Null);
		Assert.That(response.Error, Is.True);
		Assert.That(response.ErrorMessage, Is.EqualTo("Username is already taken."));
	}

	[Test]
	public async Task TestRegisterSuccess() {
		var userRegister = fixture.Create<UserRegister>();
		var registerEnabled = fixture.Create<bool>();

		var jwtIssuer = fixture.Create<string>();
		var jwtAudience = fixture.Create<string>();

		// must be 512 bits so a bit hard to set up fixture for
		var jwtKey = "MFswDQYJKoZIhvcNAQEBBQADSgAwRwJAfvsHG2cBRTa0aDJsmefeUKWke5keqwm9DJZajErHfAJjK6yTCcaxjD2skHK8uTMYIstftyl5pPKepK6qT/J08QIDAQAB";

		config.JwtIssuer.Returns(jwtIssuer);
		config.JwtAudience.Returns(jwtAudience);
		config.JwtKey.Returns(jwtKey);

		config.RegisterEnabled.Returns(registerEnabled);

		db.GetUserCountAsync().Returns(Task.FromResult(0));

		db.UserExistsAsync(userRegister.Username).Returns(Task.FromResult(false));

		var result = await controller.RegisterAsync(userRegister);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<User>, Is.True);

		var response = (Response<User>)((ObjectResult)result).Value;

		Assert.That(response.Data.Username, Is.EqualTo(userRegister.Username));
		Assert.That(response.Data.Email, Is.EqualTo(userRegister.Email));
		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
	}

	[Test]
	public async Task TestResetToken() {
		// TERRIBLE TEST
		// TODO: come back to this once logic if logic is taken out of controller
		var token = fixture.Create<string>();
		var user = fixture.Create<User>();

		var jwtIssuer = fixture.Create<string>();
		var jwtAudience = fixture.Create<string>();

		// must be 512 bits so a bit hard to set up fixture for
		var jwtKey = "MFswDQYJKoZIhvcNAQEBBQADSgAwRwJAfvsHG2cBRTa0aDJsmefeUKWke5keqwm9DJZajErHfAJjK6yTCcaxjD2skHK8uTMYIstftyl5pPKepK6qT/J08QIDAQAB";

		config.JwtIssuer.Returns(jwtIssuer);
		config.JwtAudience.Returns(jwtAudience);
		config.JwtKey.Returns(jwtKey);

		db.GetUserByTokenAsync(token).Returns(user);

		var result = await controller.ResetTokenAsync(token);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<string>, Is.True);

		var response = (Response<string>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.Not.Null);
		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
	}

	[Test]
	public async Task TestChangePasswordVerificationFail() {
		var token = fixture.Create<string>();
		var user = fixture.Create<User>();
		user.HashedPassword = "$2y$12$ibm0flWSxXHBgF40vT6mPukO.SzAOAgqK7R7/pkp6T6/7eTGw3ljO";

		var updatePassword = fixture.Create<PasswordUpdate>();

		db.GetUserByTokenAsync(token).Returns(user);

		var result = await controller.ChangePasswordAsync(token, updatePassword);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<string>, Is.True);

		var response = (Response<string>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.Null);
		Assert.That(response.Error, Is.True);
		Assert.That(response.ErrorMessage, Is.EqualTo("Failed to verify existing password."));
	}

	[Test]
	public async Task TestChangePasswordSuccess() {
		var token = fixture.Create<string>();
		var user = fixture.Create<User>();
		user.HashedPassword = "$2y$12$ibm0flWSxXHBgF40vT6mPukO.SzAOAgqK7R7/pkp6T6/7eTGw3ljO";

		var updatePassword = fixture.Create<PasswordUpdate>();
		updatePassword.Password = "test";

		db.GetUserByTokenAsync(token).Returns(user);

		var result = await controller.ChangePasswordAsync(token, updatePassword);

		Assert.That(result is ObjectResult, Is.True);
		Assert.That(((ObjectResult)result).Value is Response<string>, Is.True);

		var response = (Response<string>)((ObjectResult)result).Value;

		Assert.That(response.Data, Is.EqualTo("Successfully changed password."));
		Assert.That(response.Error, Is.False);
		Assert.That(response.ErrorMessage, Is.Null);
	}
}
