using Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.NoContent();

        return MapFailure(result.Errors, controller);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(result.Value);

        return MapFailure(result.Errors, controller);
    }

    private static IActionResult MapFailure(IReadOnlyCollection<Error> errors, ControllerBase controller)
    {
        var first = errors.First();

        return first.Type switch
        {
            ErrorType.Validation => controller.BadRequest(new ValidationProblemDetails(
                errors.GroupBy(x => x.Code).ToDictionary(g => g.Key, g => g.Select(x => x.Description).ToArray()))),
            ErrorType.NotFound => controller.NotFound(new ProblemDetails { Title = first.Description, Status = 404 }),
            ErrorType.Conflict => controller.Conflict(new ProblemDetails { Title = first.Description, Status = 409 }),
            ErrorType.Forbidden => controller.StatusCode(403, new ProblemDetails { Title = first.Description, Status = 403 }),
            ErrorType.Unauthorized => controller.Unauthorized(new ProblemDetails { Title = first.Description, Status = 401 }),
            _ => controller.BadRequest(new ProblemDetails { Title = first.Description, Status = 400 })
        };
    }
}