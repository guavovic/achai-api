using System.Text.Json.Serialization;
using Achai.Api.Common;

namespace Achai.Api.Features.Addresses;

public sealed record AddressResponse(
    [property: JsonPropertyName("cep")] string? ZipCode,
    [property: JsonPropertyName("logradouro")] string? Street,
    [property: JsonPropertyName("complemento")] string? Complement,
    [property: JsonPropertyName("unidade")] string? Unit,
    [property: JsonPropertyName("bairro")] string? Neighborhood,
    [property: JsonPropertyName("localidade")] string? City,
    [property: JsonPropertyName("uf")] string? State,
    [property: JsonPropertyName("estado")] string? StateName,
    [property: JsonPropertyName("regiao")] string? Region,
    [property: JsonPropertyName("tipoLogradouro")] string? StreetType,
    [property: JsonPropertyName("nomeLogradouro")] string? StreetName,
    [property: JsonPropertyName("enderecoFormatado")] string FormattedAddress)
{
    public static AddressResponse From(Address address)
    {
        var street = AddressFormatting.SplitStreet(address.Street);

        return new(
            address.ZipCode,
            address.Street,
            address.Complement,
            address.Unit,
            address.Neighborhood,
            address.City,
            address.State,
            address.StateName,
            address.Region,
            street.Type,
            street.Name,
            AddressFormatting.Format(address));
    }
}
