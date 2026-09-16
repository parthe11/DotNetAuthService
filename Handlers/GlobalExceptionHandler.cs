using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LoginService;

public class GlobalExceptionHandler : IExceptionHandler
{
    public GlobalExceptionHandler(){}

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ConflictException => StatusCodes.Status409Conflict,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        var response = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = GetDetail(exception, statusCode),
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            401 => "Unauthorized",
            409 => "Conflict",
            _ => "Internal Server Error"
        };
    }

    private static string GetDetail(
        Exception exception,
        int statusCode)
    {
        if (statusCode == 500)
        {
            return "An unexpected error occurred.";
        }

        return exception.Message;
    }
}
