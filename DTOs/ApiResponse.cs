namespace mini_mes_be.DTOs;

/// <summary>Unified API response wrapper.</summary>
public class ApiResponse<T>
{
    public bool status { get; set; }
    public int code { get; set; }
    public string? message { get; set; }
    public T? data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success", int code = 200) =>
        new() { status = true, code = code, message = message, data = data };

    public static ApiResponse Fail(string message, int code = 400, Dictionary<string, string>? errors = null) =>
        ApiResponse.Fail(message, code, errors);
}

public class ApiErrorData
{
    public string message { get; set; } = string.Empty;
    public Dictionary<string, string>? errors { get; set; }
}

/// <summary>Non-generic version for responses without data payload.</summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkNoData(string message = "Success", int code = 200) =>
        new() { status = true, code = code, message = message };

    public static new ApiResponse Fail(string message, int code = 400, Dictionary<string, string>? errors = null) =>
        new()
        {
            status = false,
            code = code,
            data = new ApiErrorData { message = message, errors = errors }
        };
}
