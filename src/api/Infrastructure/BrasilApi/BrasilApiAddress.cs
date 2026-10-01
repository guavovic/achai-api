using System.Text.Json.Serialization;
using Achai.Api.Common;

namespace Achai.Api.Infrastructure.BrasilApi;

public sealed class BrasilApiAddress
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("neighborhood")]
    public string? Neighborhood { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    public Address ToAddress()
    {
        var state = BrazilianStates.Find(State);

        return new Address(
            ZipCode: FormatZipCode(Cep),
            Street: Street,
            Complement: string.Empty,
            Unit: string.Empty,
            Neighborhood: Neighborhood,
            City: City,
            State: State,
            StateName: state?.Name,
            Region: state?.Region);
    }

    private static string? FormatZipCode(string? zipCode) =>
        zipCode is { Length: 8 } ? $"{zipCode[..5]}-{zipCode[5..]}" : zipCode;
}
