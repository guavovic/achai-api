using System.Text.Json.Serialization;
using Achai.Api.Common;

namespace Achai.Api.Features.Cities;

public sealed record CityResponse([property: JsonPropertyName("nome")] string? Name)
{
    public static CityResponse From(City city) => new(city.Name);
}
