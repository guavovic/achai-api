using System.Text.Json.Serialization;
using AddressLookup.Api.Common;

namespace AddressLookup.Api.Features.Cities;

// O nome no JSON fica em português porque é o contrato que o front usa.
public sealed record CityResponse([property: JsonPropertyName("nome")] string? Name)
{
    public static CityResponse From(City city) => new(city.Name);
}
