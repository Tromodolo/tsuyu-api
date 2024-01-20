global using tsuyu;
global using tsuyu.Models;
global using tsuyu.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Authentication
var jwtIssuer = Environment.GetEnvironmentVariable("JwtIssuer");
var jwtAudience = Environment.GetEnvironmentVariable("JwtAudience");
var jwtKey = Environment.GetEnvironmentVariable("JwtKey");

if (string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience)
                                    || string.IsNullOrEmpty(jwtKey)) {
	Console.WriteLine(
		"JwtIssuer, JwtAudience and JwtKey must be set as environment variables.");
	return;
}

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(opt => {
	opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt => {
	opt.TokenValidationParameters = new TokenValidationParameters {
		ValidIssuer = jwtIssuer,
		ValidAudience = jwtAudience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = false,
		ValidateIssuerSigningKey = true
	};
});

builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddSingleton<Database>(); // Depends on ConfigurationService
builder.Services.AddSingleton<FileService>(); // Depends on Database

builder.Services.AddControllers();

var app = builder.Build();

// Ensure that the static file folders exist
var publicPath = Path.Combine(
	Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "public");
var filesPath = Path.Combine(
	Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "files");

if (!Directory.Exists(publicPath)) {
	Directory.CreateDirectory(publicPath);
}
if (!Directory.Exists(filesPath)) {
	Directory.CreateDirectory(filesPath);
}

app.UseStaticFiles(new StaticFileOptions {
	FileProvider = new PhysicalFileProvider(publicPath),
	RequestPath = ""
});
app.UseStaticFiles(new StaticFileOptions {
	FileProvider = new PhysicalFileProvider(filesPath),
	RequestPath = ""
});

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.Run();
