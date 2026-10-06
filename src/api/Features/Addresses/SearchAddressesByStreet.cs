using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Achai.Api.Common;
using Achai.Api.Common.Http;
using Achai.Api.Common.Validation;
using Achai.Api.Infrastructure;

namespace Achai.Api.Features.Addresses;

public static class SearchAddressesByStreet
{
    public const string SearchedStreetHeader = "X-Logradouro-Buscado";

    public static IEndpointRouteBuilder MapSearchAddressesByStreet(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/{uf}/{cidade}/{logradouro}", HandleAsync)
            .WithName(nameof(SearchAddressesByStreet))
            .WithTags("Endereços")
            .WithSummary("Busca endereços pelo logradouro")
            .WithDescription(
                "Consulta o ViaCEP, que devolve até 50 endereços. Sem resultado, tenta variações do nome: abreviação por extenso " +
                "(Av. vira Avenida), número romano ou arábico (XV e 15) e o nome sem o tipo (Rua). Quando uma variação encontra, " +
                $"o cabeçalho {SearchedStreetHeader} diz qual foi, com o texto codificado para URL. Sem nenhum resultado, devolve uma lista vazia.")
            .Produces<List<AddressResponse>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    public static async Task<IResult> HandleAsync(
        [Description("Sigla do estado. Exemplo: SP.")][BrazilianState] string uf,
        [Description("Nome da cidade, com pelo menos 3 caracteres. Exemplo: São Paulo.")]
        [MinLength(3, ErrorMessage = "A cidade precisa de pelo menos 3 caracteres.")] string cidade,
        [Description("Parte do nome da rua, com pelo menos 3 caracteres. Exemplo: Paulista.")]
        [MinLength(3, ErrorMessage = "O logradouro precisa de pelo menos 3 caracteres.")] string logradouro,
        IAddressProvider addressProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var state = uf.ToUpperInvariant();
        var street = StreetNameVariations.Clean(logradouro);
        var result = await addressProvider.SearchByStreetAsync(state, cidade, street, cancellationToken);

        if (result.IsSuccess && result.Value.Count == 0)
        {
            foreach (var variation in StreetNameVariations.For(street))
            {
                var attempt = await addressProvider.SearchByStreetAsync(state, cidade, variation, cancellationToken);
                if (!attempt.IsSuccess)
                    break;
                if (attempt.Value.Count == 0)
                    continue;

                httpContext.Response.Headers[SearchedStreetHeader] = Uri.EscapeDataString(variation);
                result = attempt;
                break;
            }
        }

        return result.Match(
            addresses => TypedResults.Ok(addresses.Select(AddressResponse.From).ToList()),
            error => error.ToProblem());
    }
}
