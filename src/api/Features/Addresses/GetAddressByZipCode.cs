using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Achai.Api.Common.Http;
using Achai.Api.Infrastructure;

namespace Achai.Api.Features.Addresses;

public static class GetAddressByZipCode
{
    public static IEndpointRouteBuilder MapGetAddressByZipCode(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/{cep}", HandleAsync)
            .WithName(nameof(GetAddressByZipCode))
            .WithTags("Endereços")
            .WithSummary("Busca o endereço pelo CEP")
            .WithDescription("Consulta o ViaCEP. Se ele estiver fora do ar ou lento, usa a BrasilAPI. CEP que não existe devolve 404.")
            .Produces<AddressResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    public static async Task<IResult> HandleAsync(
        [Description("CEP com 8 dígitos, com ou sem traço. Exemplo: 01001000.")]
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
