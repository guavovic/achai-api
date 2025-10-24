using System.Text.Json.Serialization;

namespace BuscarEnderecos.API.Models
{    
    public class EnderecoModel
    {
        [JsonPropertyName("cep")]
        public string? CEP { get; set; }= string.Empty;

        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; set; }= string.Empty;
        
        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }= string.Empty;
        
        [JsonPropertyName("localidade")]
        public string? Localidade { get; set; }= string.Empty;
        
        [JsonPropertyName("uf")]
        public string? UF { get; set; }= string.Empty;
        
        [JsonPropertyName("estado")]
        public string? Estado { get; set; }= string.Empty;
        
        [JsonPropertyName("regiao")]
        public string? Regiao { get; set; }= string.Empty;
    }
}