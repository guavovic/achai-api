using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Achai.Api.Common.Http;
using Achai.Api.Common.Validation;
using Achai.Api.Infrastructure;

namespace Achai.Api.Features.Addresses;

public static class SearchAddressesByStreet
{
    public static IEndpointRouteBuilder MapSearchAddressesByStreet(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/{uf}/{cidade}/{logradouro}", HandleAsync)
            .WithName(nameof(SearchAddressesByStreet))
            .WithTags("Endereços")
            .WithSummary("Busca endereços pelo logradouro")
            .WithDescription("Consulta o ViaCEP, que devolve até 50 endereços. Sem resultado, devolve uma lista vazia.")
            .Produces<List<AddressResponse>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    // Os nomes dos parâmetros seguem a rota, que faz parte do contrato da API.
    public static async Task<IResult> HandleAsync(
        [Description("Sigla do estado. Exemplo: SP.")][BrazilianState] string uf,
        [Description("Nome da cidade, com pelo menos 3 caracteres. Exemplo: São Paulo.")]
        [MinLength(3, ErrorMessage = "A cidade precisa de pelo menos 3 caracteres.")] string cidade,
        [Description("Parte do nome da rua, com pelo menos 3 caracteres. Exemplo: Paulista.")]
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
