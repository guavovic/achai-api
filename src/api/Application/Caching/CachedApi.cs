using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace BuscarEnderecos.API.Caching
{
    /// <summary>
    /// Decorator do <see cref="IApi"/>: consulta o cache antes de chamar o ViaCEP ou o IBGE.
    /// Erros esperados (como CEP não encontrado) também ficam no cache. Exceções não ficam,
    /// então uma falha da API externa é tentada de novo na próxima requisição.
    /// </summary>
    public class CachedApi : IApi
    {
        private static readonly HybridCacheEntryOptions Cep = Duracao(TimeSpan.FromHours(24));
        private static readonly HybridCacheEntryOptions Logradouro = Duracao(TimeSpan.FromHours(1));
        private static readonly HybridCacheEntryOptions Cidades = Duracao(TimeSpan.FromDays(7));

        private readonly IApi _api;
        private readonly HybridCache _cache;

        public CachedApi(IApi api, HybridCache cache)
        {
            _api = api;
            _cache = cache;
        }

        public Task<Result<EnderecoModel>> BuscarEnderecoPorCEP(string cep) =>
            ObterOuBuscar($"cep:{cep}", Cep, () => _api.BuscarEnderecoPorCEP(cep));

        public Task<Result<List<EnderecoModel>>> BuscarPorEstadoECidade(string uf, string cidade, string logradouro) =>
            ObterOuBuscar($"logradouro:{uf}:{cidade}:{logradouro}", Logradouro, () => _api.BuscarPorEstadoECidade(uf, cidade, logradouro));

        public Task<Result<List<CidadeModel>>> BuscarCidadesPorUF(string uf) =>
            ObterOuBuscar($"cidades:{uf}", Cidades, () => _api.BuscarCidadesPorUF(uf));

        private async Task<Result<T>> ObterOuBuscar<T>(string chave, HybridCacheEntryOptions opcoes, Func<Task<Result<T>>> buscar)
        {
            var emCache = await _cache.GetOrCreateAsync(
                chave,
                async _ => ResultadoEmCache<T>.De(await buscar()),
                opcoes);

            return emCache.ParaResult();
        }

        private static HybridCacheEntryOptions Duracao(TimeSpan duracao) => new()
        {
            Expiration = duracao,
            LocalCacheExpiration = duracao
        };
    }
}
