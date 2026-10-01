using System.Text.Json.Serialization;
using AddressLookup.Api.Common;

namespace AddressLookup.Api.Features.Addresses;

// Os nomes no JSON ficam em português porque são o contrato que o front usa.
public sealed record AddressResponse(
    [property: JsonPropertyName("cep")] string? ZipCode,
    [property: JsonPropertyName("logradouro")] string? Street,
    [property: JsonPropertyName("complemento")] string? Complement,
    [property: JsonPropertyName("unidade")] string? Unit,
    [property: JsonPropertyName("bairro")] string? Neighborhood,
    [property: JsonPropertyName("localidade")] string? City,
    [property: JsonPropertyName("uf")] string? State,
    [property: JsonPropertyName("estado")] string? StateName,
    [property: JsonPropertyName("regiao")] string? Region)
{
    public static AddressResponse From(Address address) => new(
        address.ZipCode,
        address.Street,
        address.Complement,
        address.Unit,
        address.Neighborhood,
        address.City,
        address.State,
        address.StateName,
        address.Region);
}
