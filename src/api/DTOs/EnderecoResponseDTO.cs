namespace BuscarEnderecos.API.DTOs
{
    public class EnderecoResponseDTO
    {
        public string? CEP { get; set; }
        public string? Logradouro { get; set; }
        public string? Complemento { get; set; }
        public string? Unidade { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? UF { get; set; }
        public string? Estado { get; set; }
        public string? Regiao { get; set; }
    }
}