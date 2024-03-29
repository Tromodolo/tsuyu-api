﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace tsuyu.Controllers;

[ApiController]
[Route("user")]
public class UserController: BaseController {
	readonly IConfigurationService Config;

	public UserController(IDatabase db, IConfigurationService config) : base(db) {
		Config = config;
	}

	/// <summary>
	/// Looks up user and returns user if valid credentials. Will fail if not.
	/// </summary>
	/// <param name="userLogin">Username and password</param>
	/// <returns>Response object with authorized user</returns>
	[HttpPost]
	[Route("login")]
	public async Task<IActionResult> LoginAsync([FromBody] UserLogin userLogin) {
		var response = new Response<User>();

		var user = await Db.GetUserAsync(userLogin.Username);
		if (user != null && BCrypt.Net.BCrypt.Verify(userLogin.Password, user.HashedPassword)) {
			response.Data = user;
		} else {
			response.Error = true;
			response.ErrorMessage = "Invalid login.";
			return BadRequest(response);
		}

		return Ok(response);
	}

	[HttpPost]
	[Route("register")]
	public async Task<IActionResult> RegisterAsync([FromBody] UserRegister userRegister) {
		var response = new Response<User>();
		var registerEnabled = Config.RegisterEnabled;

		// If no one has registered yet, allow registration of a single user
		var userCount = await Db.GetUserCountAsync();
		if (userCount == 0) {
			registerEnabled = true;
		}

		if (!registerEnabled) {
			response.Error = true;
			response.ErrorMessage = "Registering is disabled on this instance.";
			return Unauthorized(response);
		}
		var userNameExists = await Db.UserExistsAsync(userRegister.Username);
		if (userNameExists) {
			response.Error = true;
			response.ErrorMessage = "Username is already taken.";
			return BadRequest(response);
		}

		// Hash iterations are 2 ^ workFactor, meaning 12 would mean
		// 4096 iterations of the hash algorithm.
		// 12 is a good middle ground that isn't too slow.
		var hashWorkFactor = 12;
		var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegister.Password, hashWorkFactor);

		var apiToken = GenerateToken(userRegister.Username);

		var newUser = new User {
			Username = userRegister.Username,
			Email = userRegister.Email,
			HashedPassword = hashedPassword,
			ApiToken = apiToken,
			IsAdmin = false,
		};
		await Db.CreateUserAsync(newUser);

		response.Data = newUser;
		return Ok(response);
	}

	[HttpPost]
	[Route("reset-token")]
	[Authorize]
	public async Task<IActionResult> ResetTokenAsync([FromHeader] string authorization) {
		var response = new Response<string>();

		var authenticatedUser = await GetAuthenticatedUserAsync(authorization);
		var newToken = GenerateToken(authenticatedUser.Username);
		await Db.SetApiTokenForUserIdAsync(authenticatedUser.Id, newToken);

		response.Data = newToken;
		return Ok(response);
	}

	[HttpPost]
	[Route("change-password")]
	[Authorize]
	public async Task<IActionResult> ChangePasswordAsync([FromHeader] string authorization, [FromBody] PasswordUpdate passwordUpdate) {
		var response = new Response<string>();
		var authenticatedUser = await GetAuthenticatedUserAsync(authorization);

		if (!BCrypt.Net.BCrypt.Verify(passwordUpdate.Password, authenticatedUser.HashedPassword)) {
			response.Error = true;
			response.ErrorMessage = "Failed to verify existing password.";
			return BadRequest(response);
		}

		// Hash iterations are 2 ^ workFactor, meaning 12 would mean
		// 4096 iterations of the hash algorithm.
		// 12 is a good middle ground that isn't too slow.
		var hashWorkFactor = 12;
		var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordUpdate.NewPassword, hashWorkFactor);
		await Db.SetNewPasswordForUserIdAsync(authenticatedUser.Id, newPasswordHash);

		response.Data = "Successfully changed password.";
		return Ok(response);
	}

	/// <summary>
	/// Generates an API token for a specific username.
	/// </summary>
	/// <remarks>Token lasts 100 years (because permanent is impossible).</remarks>
	/// <param name="userName">Username to generate token for.</param>
	/// <returns>API Token</returns>
	string GenerateToken(string userName) {
		// Note: This token is meant to be permanent and the expire-time should
		// never be changed to something lower.
		// Reason for this is because it would be unusable in something like ShareX,
		// where you don't want to update your token all the time.
		// TODO: Possibly separate upload token from auth token?
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Config.JwtKey));
		var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
		var token = tokenHandler.CreateToken(new SecurityTokenDescriptor {
			Subject = new ClaimsIdentity(new[] {
				new Claim("Id", Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Sub, userName),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			}),
			Expires = DateTime.UtcNow.AddYears(100),
			Issuer = Config.JwtIssuer,
			Audience = Config.JwtAudience,
			SigningCredentials = signingCredentials
		});
		return tokenHandler.WriteToken(token);
	}
}
