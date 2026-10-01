using System.ComponentModel.DataAnnotations;
using AddressLookup.Api.Common.Http;
using AddressLookup.Api.Common.Validation;
using AddressLookup.Api.Infrastructure;

namespace AddressLookup.Api.Features.Addresses;

public static class SearchAddressesByStreet
{
    public static IEndpointRouteBuilder MapSearchAddressesByStreet(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/{uf}/{cidade}/{logradouro}", HandleAsync)
            .WithName(nameof(SearchAddressesByStreet))
            .WithTags("Endereços");

        return app;
    }

    // Os nomes dos parâmetros seguem a rota, que faz parte do contrato da API.
    public static async Task<IResult> HandleAsync(
        [BrazilianState] string uf,
        [MinLength(3, ErrorMessage = "A cidade precisa de pelo menos 3 caracteres.")] string cidade,
        [MinLength(3, ErrorMessage = "O logradouro precisa de pelo menos 3 caracteres.")] string logradouro,
        IAddressProvider addressProvider,
        CancellationToken cancellationToken)
    {
        var result = await addressProvider.SearchByStreetAsync(uf.ToUpperInvariant(), cidade, logradouro, cancellationToken);

        return result.Match(
            addresses => TypedResults.Ok(addresses.Select(AddressResponse.From).ToList()),
            error => error.ToProblem());
    }
}
