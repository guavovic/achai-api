using System.ComponentModel;
using System.Text.RegularExpressions;
using Achai.Api.Common;
using Achai.Api.Common.Results;
using Achai.Api.Features.Addresses;
using Achai.Api.Features.Cities;
using Achai.Api.Features.Geo;
using Achai.Api.Infrastructure;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Achai.Api.Features.Mcp;

[McpServerToolType]
public static partial class AddressTools
{
    [McpServerTool(Name = "buscar_cep", ReadOnly = true, OpenWorld = true)]
    [Description("Busca o endereço brasileiro de um CEP (logradouro, bairro, cidade, UF e uma linha formatada).")]
    public static async Task<AddressResponse> SearchZipCodeAsync(
        [Description("CEP com 8 dígitos, com ou sem traço. Exemplo: 01001-000.")] string cep,
        IAddressProvider addresses,
        CancellationToken cancellationToken)
    {
        var result = await addresses.GetByZipCodeAsync(ZipCode(cep), cancellationToken);
        return AddressResponse.From(Unwrap(result));
    }

    [McpServerTool(Name = "buscar_logradouro", ReadOnly = true, OpenWorld = true)]
    [Description(
        "Busca até 50 endereços pelo nome da rua numa cidade. Aceita parte do nome e tenta variações " +
        "(abreviação por extenso, número romano ou arábico, nome sem o tipo) quando não acha.")]
    public static async Task<List<AddressResponse>> SearchStreetAsync(
        [Description("Sigla do estado. Exemplo: SP.")] string uf,
        [Description("Nome da cidade. Exemplo: São Paulo.")] string cidade,
        [Description("Nome ou parte do nome da rua, com pelo menos 3 caracteres. Exemplo: Paulista.")] string logradouro,
        IAddressProvider addresses,
        CancellationToken cancellationToken)
    {
        var state = State(uf);
        var street = StreetNameVariations.Clean(logradouro);
        if (cidade.Trim().Length < 3 || street.Length < 3)
            throw new McpException("Cidade e logradouro precisam de pelo menos 3 caracteres.");

        foreach (var attempt in StreetNameVariations.For(street).Prepend(street))
        {
            var found = Unwrap(await addresses.SearchByStreetAsync(state, cidade.Trim(), attempt, cancellationToken));
            if (found.Count > 0)
                return found.Select(AddressResponse.From).ToList();
        }

        return [];
    }

    [McpServerTool(Name = "listar_cidades", ReadOnly = true, OpenWorld = true)]
    [Description("Lista os nomes das cidades de um estado brasileiro, pelo IBGE.")]
    public static async Task<List<CityResponse>> ListCitiesAsync(
        [Description("Sigla do estado. Exemplo: SC.")] string uf,
        ICityProvider cities,
        CancellationToken cancellationToken)
    {
        var result = await cities.GetByStateAsync(State(uf), cancellationToken);
        return Unwrap(result).Select(CityResponse.From).ToList();
    }

    [McpServerTool(Name = "distancia_entre_ceps", ReadOnly = true, OpenWorld = true)]
    [Description(
        "Distância em linha reta, em km, entre dois CEPs. As coordenadas costumam ser o centro da cidade, " +
        "então serve para distância entre cidades, não dentro da mesma cidade.")]
    public static async Task<DistanceResponse> DistanceAsync(
        [Description("CEP de origem. Exemplo: 01001-000.")] string origem,
        [Description("CEP de destino. Exemplo: 20040-020.")] string destino,
        ICoordinatesProvider coordinates,
        CancellationToken cancellationToken)
    {
        var (origin, destination) = (ZipCode(origem), ZipCode(destino));
        var from = Unwrap(await coordinates.GetCoordinatesAsync(origin, cancellationToken));
        var to = Unwrap(await coordinates.GetCoordinatesAsync(destination, cancellationToken));

        return new DistanceResponse(
            new CoordinatesResponse(origin, from.Latitude, from.Longitude, "BrasilAPI"),
            new CoordinatesResponse(destination, to.Latitude, to.Longitude, "BrasilAPI"),
            Math.Round(from.DistanceKmTo(to), 2));
    }

    private static string ZipCode(string cep) =>
        ZipCodeFormat().IsMatch(cep.Trim())
            ? cep.Trim().Replace("-", "")
            : throw new McpException("O CEP deve ter 8 dígitos, com ou sem traço.");

    private static string State(string uf) =>
        BrazilianStates.Find(uf.Trim())?.Code ?? throw new McpException($"\"{uf}\" não é a sigla de um estado brasileiro.");

    private static T Unwrap<T>(Result<T> result) =>
        result.IsSuccess ? result.Value : throw new McpException(result.Error!.Description);

    [GeneratedRegex(@"^\d{5}-?\d{3}$")]
    private static partial Regex ZipCodeFormat();
}
