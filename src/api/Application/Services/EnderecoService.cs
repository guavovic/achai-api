using AutoMapper;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.DTOs;

namespace BuscarEnderecos.API.Services
{
    public class EnderecoService : IEnderecoService
    {
        private readonly IMapper _mapper;
        private readonly IApi _api;

        public EnderecoService(IMapper mapper, IApi api)
        {
            _mapper = mapper;
            _api = api;
        }

        public async Task<ResponseDTO<EnderecoResponseDTO>> BuscarEnderecoPorCEP(string cep)
        {
            var endereco = await _api.BuscarEnderecoPorCEP(cep);
            return _mapper.Map<ResponseDTO<EnderecoResponseDTO>>(endereco);
        }

        public async Task<ResponseDTO<List<EnderecoResponseDTO>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro)
        {
            var enderecos = await _api.BuscarPorEstadoECidade(uf, cidade, logradouro);
            return _mapper.Map<ResponseDTO<List<EnderecoResponseDTO>>>(enderecos);
        }

        public async Task<ResponseDTO<List<CidadeResponseDTO>>> BuscarCidadesPorUF(string uf)
        {
            var cidades = await _api.BuscarCidadesPorUF(uf);
            return _mapper.Map<ResponseDTO<List<CidadeResponseDTO>>>(cidades);
        }
    }
}