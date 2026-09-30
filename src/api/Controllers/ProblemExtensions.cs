using BuscarEnderecos.API.Results;
using Microsoft.AspNetCore.Mvc;

namespace BuscarEnderecos.API.Controllers
{
    public static class ProblemExtensions
    {
        public static IActionResult ToProblem(this ControllerBase controller, Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            var problem = controller.ProblemDetailsFactory.CreateProblemDetails(
                controller.HttpContext,
                statusCode: statusCode,
                detail: error.Description);

            problem.Extensions["code"] = error.Code;

            return new ObjectResult(problem) { StatusCode = statusCode };
        }
    }
}
