using System.Text.Json;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Results;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BuscarEnderecos.API.Rest
{
    /// <summary>
    /// Decorator do <see cref="IApi"/>: se a busca por CEP no ViaCEP falhar (fora do ar, lento ou com o
    /// circuito aberto), tenta a fonte de reserva. Um "não encontrado" do ViaCEP é resposta válida e não
    /// aciona a reserva, porque as fontes discordam sobre CEPs que não existem.
    /// </summary>
    public class FallbackApi : IApi
    {
        private readonly IApi _api;
        private readonly ICepReserva _reserva;
        private readonly ILogger<FallbackApi> _logger;

        public FallbackApi(IApi api, ICepReserva reserva, ILogger<FallbackApi> logger)
        {
            _api = api;
            _reserva = reserva;
            _logger = logger;
        }

        public async Task<Result<EnderecoModel>> BuscarEnderecoPorCEP(string cep)
        {
            try
            {
                return await _api.BuscarEnderecoPorCEP(cep);
            }
            catch (Exception ex) when (FalhaDaApiExterna(ex))
            {
                _logger.LogWarning(ex, "ViaCEP falhou ao buscar o CEP {Cep}. Usando a fonte de reserva.", cep);
                return await _reserva.BuscarEnderecoPorCEP(cep);
            }
        }

        public Task<Result<List<EnderecoModel>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro) =>
            _api.BuscarPorEstadoECidade(uf, cidade, logradouro);

        public Task<Result<List<CidadeModel>>> BuscarCidadesPorUF(string uf) =>
            _api.BuscarCidadesPorUF(uf);

        private static bool FalhaDaApiExterna(Exception ex) =>
            ex is HttpRequestException or TimeoutRejectedException or BrokenCircuitException or JsonException;
    }
}
