using System.ComponentModel.DataAnnotations;
using AddressLookup.Api.Common.Http;
using AddressLookup.Api.Infrastructure;

namespace AddressLookup.Api.Features.Addresses;

public static class GetAddressByZipCode
{
    public static IEndpointRouteBuilder MapGetAddressByZipCode(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/{cep}", HandleAsync)
            .WithName(nameof(GetAddressByZipCode))
            .WithTags("Endereços");

        return app;
    }

    // O nome do parâmetro segue a rota, que faz parte do contrato da API.
    public static async Task<IResult> HandleAsync(
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP deve ter 8 dígitos, com ou sem traço.")] string cep,
        IAddressProvider addressProvider,
        CancellationToken cancellationToken)
    {
        var result = await addressProvider.GetByZipCodeAsync(cep.Replace("-", ""), cancellationToken);

        return result.Match(
            address => TypedResults.Ok(AddressResponse.From(address)),
            error => error.ToProblem());
    }
}
