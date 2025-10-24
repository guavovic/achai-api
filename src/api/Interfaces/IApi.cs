using BuscarEnderecos.API.DTOs;
using BuscarEnderecos.API.Models;

namespace BuscarEnderecos.API.Interfaces
{
    public interface IApi
    {
        Task<ResponseDTO<EnderecoModel>> BuscarEnderecoPorCEP(string cep);
        Task<ResponseDTO<List<EnderecoModel>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro);
        Task<ResponseDTO<List<CidadeModel>>> BuscarCidadesPorUF(string uf);
    }
}