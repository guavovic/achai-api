using BuscarEnderecos.API.DTOs;
using BuscarEnderecos.API.Results;

namespace BuscarEnderecos.API.Interfaces
{
    public interface IEnderecoService
    {
        Task<Result<EnderecoResponseDTO>> BuscarEnderecoPorCEP(string cep);
        Task<Result<List<EnderecoResponseDTO>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro);
        Task<Result<List<CidadeResponseDTO>>> BuscarCidadesPorUF(string uf);
    }
}