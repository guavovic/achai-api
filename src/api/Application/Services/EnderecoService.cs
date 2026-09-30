using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.DTOs;
using BuscarEnderecos.API.Mapping;

namespace BuscarEnderecos.API.Services
{
    public class EnderecoService : IEnderecoService
    {
        private readonly IApi _api;

        public EnderecoService(IApi api)
        {
            _api = api;
        }

        public async Task<ResponseDTO<EnderecoResponseDTO>> BuscarEnderecoPorCEP(string cep)
        {
            var endereco = await _api.BuscarEnderecoPorCEP(cep);
            return endereco.Map(e => e.ToDto());
        }

        public async Task<ResponseDTO<List<EnderecoResponseDTO>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro)
        {
            var enderecos = await _api.BuscarPorEstadoECidade(uf, cidade, logradouro);
            return enderecos.Map(lista => lista.Select(e => e.ToDto()).ToList());
        }

        public async Task<ResponseDTO<List<CidadeResponseDTO>>> BuscarCidadesPorUF(string uf)
        {
            var cidades = await _api.BuscarCidadesPorUF(uf);
            return cidades.Map(lista => lista.Select(c => c.ToDto()).ToList());
        }
    }
}
