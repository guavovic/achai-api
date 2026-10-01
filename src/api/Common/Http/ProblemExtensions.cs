using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Common.Http;

public static class ProblemExtensions
{
    public static IResult ToProblem(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Problem(
            statusCode: statusCode,
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }

    /// <summary>
    /// Registra o ProblemDetails trocando os títulos padrão (em inglês) de toda resposta de erro da API.
    /// </summary>
    public static IServiceCollection AddPortugueseProblemDetails(this IServiceCollection services) =>
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var problem = context.ProblemDetails;

                problem.Title = problem switch
                {
                    HttpValidationProblemDetails => "Um ou mais campos são inválidos.",
                    { Status: StatusCodes.Status400BadRequest } => "Requisição inválida",
                    { Status: StatusCodes.Status404NotFound } => "Não encontrado",
                    _ => problem.Title
                };
            };
        });
}
