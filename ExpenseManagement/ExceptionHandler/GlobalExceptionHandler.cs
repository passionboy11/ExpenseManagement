using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.ExceptionHandler;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unexpected error occurred");
        string? currentId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        Dictionary<string, object> otherDetails = new()
        {
            { "CurrentId", currentId },
            { "TraceId", Convert.ToString(Activity.Current!.Context.TraceId)! }
        };
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = exception.GetType().Name,
                Title = "An unexpected error occurred",
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                Extensions = otherDetails!
            },
            cancellationToken
        );
        return true;
    }
}
