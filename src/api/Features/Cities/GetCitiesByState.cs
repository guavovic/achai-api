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
            .WithTags("Cidades");

        return app;
    }

    // O nome do parâmetro segue a rota, que faz parte do contrato da API.
    public static async Task<IResult> HandleAsync(
        [BrazilianState] string uf,
        ICityProvider cityProvider,
        CancellationToken cancellationToken)
    {
        var result = await cityProvider.GetByStateAsync(uf.ToUpperInvariant(), cancellationToken);

        return result.Match(
            cities => TypedResults.Ok(cities.Select(CityResponse.From).ToList()),
            error => error.ToProblem());
    }
}
