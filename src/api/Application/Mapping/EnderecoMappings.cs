using BuscarEnderecos.API.DTOs;
using BuscarEnderecos.API.Models;

namespace BuscarEnderecos.API.Mapping
{
    public static class EnderecoMappings
    {
        public static EnderecoResponseDTO ToDto(this EnderecoModel model) => new()
        {
            CEP = model.CEP,
            Logradouro = model.Logradouro,
            Complemento = model.Complemento,
            Unidade = model.Unidade,
            Bairro = model.Bairro,
            Localidade = model.Localidade,
            UF = model.UF,
            Estado = model.Estado,
            Regiao = model.Regiao
        };

        public static CidadeResponseDTO ToDto(this CidadeModel model) => new()
        {
            Nome = model.Nome
        };
    }
}
