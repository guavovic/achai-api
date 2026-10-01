using System.Text.Json.Serialization;

namespace AddressLookup.Api.Infrastructure.Ibge;

public sealed class IbgeCity
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }
}
