using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Achai.Api.Common.Results;
using Achai.Api.Infrastructure;

namespace Achai.Api.Features.Addresses;

public sealed record BatchRequest(
    [property: JsonPropertyName("ceps")]
    [property: Description("De 1 a 20 CEPs, com ou sem traço.")]
    [property: Required(ErrorMessage = "Envie a lista de CEPs.")]
    [property: Length(1, GetAddressesByZipCodes.MaxZipCodes, ErrorMessage = "Envie de 1 a 20 CEPs.")]
    List<string> ZipCodes);

public sealed record BatchItem(
    [property: JsonPropertyName("cep")] string ZipCode,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("endereco")] AddressResponse? Address);

public static partial class GetAddressesByZipCodes
{
    public const int MaxZipCodes = 20;
    private const int MaxConcurrency = 4;

    public static IEndpointRouteBuilder MapGetAddressesByZipCodes(this IEndpointRouteBuilder app)
    {
        app.MapPost("/buscar/lote", HandleAsync)
            .WithName(nameof(GetAddressesByZipCodes))
            .WithTags("Endereços")
            .WithSummary("Busca vários CEPs de uma vez")
            .WithDescription(
                "Recebe de 1 a 20 CEPs e devolve um item para cada um, na mesma ordem, com o status " +
                "encontrado, nao_encontrado ou invalido. Usa o mesmo cache e o mesmo fallback da busca por CEP.")
            .Produces<List<BatchItem>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    public static async Task<IResult> HandleAsync(
        BatchRequest request,
        IAddressProvider addressProvider,
        CancellationToken cancellationToken)
    {
        using var gate = new SemaphoreSlim(MaxConcurrency);

        var items = await Task.WhenAll(request.ZipCodes.Select(async zipCode =>
        {
            if (!ZipCodeFormat().IsMatch(zipCode ?? ""))
                return new BatchItem(zipCode ?? "", "invalido", null);

            await gate.WaitAsync(cancellationToken);
            try
            {
                var result = await addressProvider.GetByZipCodeAsync(zipCode!.Replace("-", ""), cancellationToken);
                return result.Match(
                    address => new BatchItem(zipCode, "encontrado", AddressResponse.From(address)),
                    error => new BatchItem(zipCode, error.Type == ErrorType.NotFound ? "nao_encontrado" : "invalido", null));
            }
            finally
            {
                gate.Release();
            }
        }));

        return TypedResults.Ok(items.ToList());
    }

    [GeneratedRegex(@"^\d{5}-?\d{3}$")]
    private static partial Regex ZipCodeFormat();
}
