using System.Text.Json.Serialization;

namespace BuscarEnderecos.API.Models
{    
    public class CidadeModel
    {
        [JsonPropertyName("nome")]
        public string? Nome { get; set; }= string.Empty;
    }
}