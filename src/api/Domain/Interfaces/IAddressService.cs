using BuscarEnderecos.API.DTOs;

namespace BuscarEnderecos.API.Interfaces
{
    public interface IEnderecoService
    {
        Task<ResponseDTO<EnderecoResponseDTO>> BuscarEnderecoPorCEP(string cep);
        Task<ResponseDTO<List<EnderecoResponseDTO>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro);
        Task<ResponseDTO<List<CidadeResponseDTO>>> BuscarCidadesPorUF(string uf);
    }
}