using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Interfaces
{
    public interface IApi
    {
        Task<Result<EnderecoModel>> BuscarEnderecoPorCEP(string cep);
        Task<Result<List<EnderecoModel>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro);
        Task<Result<List<CidadeModel>>> BuscarCidadesPorUF(string uf);
    }
}