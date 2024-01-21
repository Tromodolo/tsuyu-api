namespace tsuyu.Models;

/// <summary>
/// Used to give a structured response to *queries*, like files/list.
/// Returns a 'Cursor' value which can be used in the queries to paginate.
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryResponse<T> {
	public T Data { get; set; }
	public int TotalItems { get; set; }
	/// <summary>
	/// Id of the last item in the response.
	/// </summary>
	public string? Cursor { get; set; }
	public bool Error { get; set; }
	public string? ErrorMessage { get; set; }

	public QueryResponse(){}

	public QueryResponse(T data, bool error = false, string? errorMessage = null) {
		Data = data;
		Error = error;
		ErrorMessage = errorMessage;
	}
}
