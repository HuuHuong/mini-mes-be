namespace mini_mes_be.DTOs;

/// <summary>Unified API response wrapper.</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>Non-generic version for responses without data payload.</summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkNoData(string message = "Success") =>
        new() { Success = true, Message = message };

    public static new ApiResponse Fail(string message) =>
        new() { Success = false, Message = message };
}
