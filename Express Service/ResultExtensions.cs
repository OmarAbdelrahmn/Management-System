using Application.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Express_Service;

public static class ResultExtensions
{
    public static IActionResult ToProblem(this Result result)
    {
        var statusCode = result.Error.StatuesCode ?? StatusCodes.Status400BadRequest;
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = result.Error.Code,
            Detail = result.Error.Description
        };
        problem.Extensions["error"] = new
        {
            result.Error.Code,
            result.Error.Description
        };

        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
