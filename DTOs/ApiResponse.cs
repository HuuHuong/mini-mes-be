namespace mini_mes_be.DTOs;

/// <summary>Unified API response wrapper.</summary>
public class ApiResponse<T>
{
    public bool Status { get; set; }
    public int Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success", int code = 200) =>
        new() { Status = true, Code = code, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, int code = 400) =>
        new() { Status = false, Code = code, Message = message };
}

/// <summary>Non-generic version for responses without data payload.</summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkNoData(string message = "Success", int code = 200) =>
        new() { Status = true, Code = code, Message = message };

    public static new ApiResponse Fail(string message, int code = 400) =>
        new() { Status = false, Code = code, Message = message };
}
