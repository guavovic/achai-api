using System.Text.Json;
using System.Text.Json.Serialization;
using Achai.Api.Common;

namespace Achai.Api.Infrastructure.ViaCep;

public sealed class ViaCepAddress
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("unidade")]
    public string? Unidade { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("localidade")]
    public string? Localidade { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("estado")]
    public string? Estado { get; set; }

    [JsonPropertyName("regiao")]
    public string? Regiao { get; set; }

    // Só vem quando o CEP não existe. Já foi booleano e hoje é a string "true".
    [JsonPropertyName("erro")]
    public JsonElement? Erro { get; set; }

    public bool NotFound => Erro is not null;

    public Address ToAddress() =>
        new(Cep, Logradouro, Complemento, Unidade, Bairro, Localidade, Uf, Estado, Regiao);
}
