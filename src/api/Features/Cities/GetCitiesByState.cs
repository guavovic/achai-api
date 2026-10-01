using System.ComponentModel;
using Achai.Api.Common.Http;
using Achai.Api.Common.Validation;
using Achai.Api.Infrastructure;

namespace Achai.Api.Features.Cities;

public static class GetCitiesByState
{
    public static IEndpointRouteBuilder MapGetCitiesByState(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/cidades/{uf}", HandleAsync)
            .WithName(nameof(GetCitiesByState))
            .WithTags("Cidades")
            .WithSummary("Lista as cidades de um estado")
            .WithDescription("Consulta o IBGE. Estado que não está entre as 27 siglas devolve 400.")
            .Produces<List<CityResponse>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    public static async Task<IResult> HandleAsync(
        [Description("Sigla do estado. Exemplo: SC.")][BrazilianState] string uf,
        ICityProvider cityProvider,
        CancellationToken cancellationToken)
    {
        var result = await cityProvider.GetByStateAsync(uf.ToUpperInvariant(), cancellationToken);

        return result.Match(
            cities => TypedResults.Ok(cities.Select(CityResponse.From).ToList()),
            error => error.ToProblem());
    }
}
