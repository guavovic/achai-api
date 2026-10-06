using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Achai.Api.Common;
using Achai.Api.Common.Results;
using Achai.Api.Infrastructure;
using Achai.Api.Infrastructure.BrasilApi;
using Achai.Api.Infrastructure.ViaCep;

namespace Achai.Api.Features.Addresses;

public sealed record SourceResult(
    [property: JsonPropertyName("fonte")] string Source,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("endereco")] AddressResponse? Address);

public sealed record ConsensusResponse(
    [property: JsonPropertyName("cep")] string ZipCode,
    [property: JsonPropertyName("concordam")] bool Agree,
    [property: JsonPropertyName("divergencias")] List<string> Differences,
    [property: JsonPropertyName("fontes")] List<SourceResult> Sources);

public static class CompareZipCodeSources
{
    private static readonly (string Name, Func<Address, string?> Value)[] ComparedFields =
    [
        ("logradouro", address => address.Street),
        ("bairro", address => address.Neighborhood),
        ("localidade", address => address.City),
        ("uf", address => address.State),
    ];

    public static IEndpointRouteBuilder MapCompareZipCodeSources(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/{cep}/consenso", HandleAsync)
            .WithName(nameof(CompareZipCodeSources))
            .WithTags("Endereços")
            .WithSummary("Compara o CEP no ViaCEP e na BrasilAPI")
            .WithDescription(
                "Consulta as duas fontes ao mesmo tempo, sem cache, e diz se concordam. Compara logradouro, bairro, " +
                "localidade e UF, sem diferença de acentos e maiúsculas. Quando uma fonte acha o CEP e a outra não, " +
                "a divergência é \"encontrado\".")
            .Produces<ConsensusResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    public static async Task<IResult> HandleAsync(
        [Description("CEP com 8 dígitos, com ou sem traço. Exemplo: 01001000.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "O CEP deve ter 8 dígitos, com ou sem traço.")] string cep,
        ViaCepClient viaCep,
        BrasilApiClient brasilApi,
        CancellationToken cancellationToken)
    {
        var zipCode = cep.Replace("-", "");
        var results = await Task.WhenAll(
            QueryAsync("ViaCEP", viaCep, zipCode, cancellationToken),
            QueryAsync("BrasilAPI", brasilApi, zipCode, cancellationToken));

        var found = results.Where(result => result.Address is not null).Select(result => result.Address!).ToList();
        var differences = found.Count switch
        {
            2 => ComparedFields.Where(field => TextFolding.Fold(field.Value(found[0])) != TextFolding.Fold(field.Value(found[1]))).Select(field => field.Name).ToList(),
            1 => ["encontrado"],
            _ => [],
        };

        return TypedResults.Ok(new ConsensusResponse(
            zipCode,
            found.Count == 2 && differences.Count == 0,
            differences,
            results.Select(result => result.Source).ToList()));
    }

    private static async Task<(SourceResult Source, Address? Address)> QueryAsync(
        string name, IZipCodeProvider provider, string zipCode, CancellationToken cancellationToken)
    {
        try
        {
            var result = await provider.GetByZipCodeAsync(zipCode, cancellationToken);
            return result.Match(
                address => (new SourceResult(name, "encontrado", AddressResponse.From(address)), address),
                error => (new SourceResult(name, error.Type == ErrorType.NotFound ? "nao_encontrado" : "invalido", null), (Address?)null));
        }
        catch (Exception exception) when (exception is HttpRequestException or TimeoutException or Polly.CircuitBreaker.BrokenCircuitException or OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return (new SourceResult(name, "indisponivel", null), null);
        }
    }
}
