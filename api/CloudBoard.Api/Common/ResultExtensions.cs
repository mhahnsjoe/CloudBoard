using Microsoft.AspNetCore.Mvc;

namespace CloudBoard.Api.Common;

/// <summary>
/// Extension methods for converting Result to IActionResult
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to appropriate IActionResult based on success/failure
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return result.StatusCode switch
        {
            404 => new NotFoundObjectResult(new { error = result.Error }),
            403 => new ForbidResult(),
            401 => new UnauthorizedObjectResult(new { error = result.Error }),
            _ => new BadRequestObjectResult(new { error = result.Error })
        };
    }

    /// <summary>
    /// Converts a Result<T> to appropriate IActionResult with value payload
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.StatusCode switch
        {
            404 => new NotFoundObjectResult(new { error = result.Error }),
            403 => new ForbidResult(),
            401 => new UnauthorizedObjectResult(new { error = result.Error }),
            _ => new BadRequestObjectResult(new { error = result.Error })
        };
    }

    /// <summary>
    /// Converts Result<T> to CreatedAtAction result for POST endpoints
    /// </summary>
    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        ControllerBase controller,
        string actionName,
        object routeValues)
    {
        if (result.IsSuccess)
            return controller.CreatedAtAction(actionName, routeValues, result.Value);

        return result.ToActionResult();
    }
}
