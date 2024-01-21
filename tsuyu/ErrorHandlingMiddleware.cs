using Microsoft.AspNetCore.Mvc;

namespace tsuyu;

public class ErrorHandlingMiddleware {
	readonly RequestDelegate _next;
	readonly ILogger<ErrorHandlingMiddleware> _logger;

	public ErrorHandlingMiddleware(RequestDelegate next,
		ILogger<ErrorHandlingMiddleware> logger){
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context) {
		try {
			await _next(context);
		} catch (Exception exception) {
			_logger.LogError(exception,
				"Unhandled exception happened: {Message}", exception.Message);

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			await context.Response.WriteAsJsonAsync(new Response<string?>(
				null,
				true,
				$"Internal Error: {exception}")
		);
		}
	}

}
