using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Infrastructure;

/// <summary>
/// Categorizes a service-layer failure so controllers can map it to the
/// correct HTTP status code instead of always returning 400.
/// </summary>
public enum ErrorType
{
    /// <summary>Bad input / validation problem. Maps to 400.</summary>
    Validation,

    /// <summary>Not authenticated, or credentials are wrong. Maps to 401.</summary>
    Unauthorized,

    /// <summary>Authenticated but not allowed to do this. Maps to 403.</summary>
    Forbidden,

    /// <summary>The requested resource doesn't exist. Maps to 404.</summary>
    NotFound,

    /// <summary>The resource already exists / state conflict. Maps to 409.</summary>
    Conflict,

    /// <summary>Unexpected failure (exception, etc). Maps to 500.</summary>
    ServerError
}

public static class ErrorResponseExtensions
{
    /// <summary>
    /// Turns a service failure (ErrorType + message) into the appropriately
    /// coded ActionResult, instead of every failure being a 400.
    /// Declared as ActionResult (not IActionResult) so it implicitly converts
    /// both when a controller action returns ActionResult and when it returns
    /// ActionResult&lt;T&gt;.
    /// </summary>
    public static ActionResult ToErrorResult(this ControllerBase controller, ErrorType errorType, string message)
    {
        var payload = new { Message = message };

        return errorType switch
        {
            ErrorType.Unauthorized => controller.Unauthorized(payload),
            ErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, payload),
            ErrorType.NotFound => controller.NotFound(payload),
            ErrorType.Conflict => controller.Conflict(payload),
            ErrorType.ServerError => controller.StatusCode(StatusCodes.Status500InternalServerError, payload),
            _ => controller.BadRequest(payload)
        };
    }
}