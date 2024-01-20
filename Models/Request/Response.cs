namespace tsuyu.Models;

/// <summary>
/// Used to give structured responses to API requests.
/// Queries should use QueryResponse.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T> {
	public T Data { get; set; }
	public bool Error { get; set; }
	public string? ErrorMessage { get; set; }

	public Response(){}

	public Response(T data, bool error = false, string? errorMessage = null) {
		Data = data;
		Error = error;
		ErrorMessage = errorMessage;
	}
}
