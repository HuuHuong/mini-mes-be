using System.Net;
using mini_mes_be.Extensions;

namespace mini_mes_be.Middlewares;

public class AppValidationException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public Dictionary<string, string> Errors { get; }

    public AppValidationException(string message, string field, string fieldError, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = new Dictionary<string, string> { { field.ToSnakeCase(), fieldError } };
    }

    public AppValidationException(string message, Dictionary<string, string> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors.ToDictionary(kvp => kvp.Key.ToSnakeCase(), kvp => kvp.Value);
    }
}
