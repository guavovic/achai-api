using System.Text.Json.Serialization;

namespace Achai.Api.Infrastructure.Ibge;

public sealed class IbgeCity
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }
}
