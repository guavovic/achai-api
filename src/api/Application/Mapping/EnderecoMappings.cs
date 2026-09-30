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

        public static ResponseDTO<TOut> Map<TIn, TOut>(this ResponseDTO<TIn> source, Func<TIn, TOut> map)
            where TIn : class
            where TOut : class => new()
        {
            HttpCode = source.HttpCode,
            ResponseError = source.ResponseError,
            ResponseData = source.ResponseData is null ? null : map(source.ResponseData)
        };
    }
}
