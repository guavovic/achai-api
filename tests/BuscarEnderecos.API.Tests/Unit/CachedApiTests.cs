using BuscarEnderecos.API.Caching;
using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class CachedApiTests : IDisposable
    {
        private readonly IApi _api = Substitute.For<IApi>();
        private readonly ServiceProvider _provider;
        private readonly CachedApi _cachedApi;

        public CachedApiTests()
        {
            _provider = new ServiceCollection().AddHybridCache().Services.BuildServiceProvider();
            _cachedApi = new CachedApi(_api, _provider.GetRequiredService<HybridCache>());
        }

        public void Dispose() => _provider.Dispose();

        [Fact]
        public async Task BuscarEnderecoPorCEP_NaSegundaVez_UsaOCache()
        {
            _api.BuscarEnderecoPorCEP("01001000").Returns(new EnderecoModel { Logradouro = "Praça da Sé" });

            await _cachedApi.BuscarEnderecoPorCEP("01001000");
            var result = await _cachedApi.BuscarEnderecoPorCEP("01001000");

            result.Value.Logradouro.ShouldBe("Praça da Sé");
            await _api.Received(1).BuscarEnderecoPorCEP("01001000");
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_CepsDiferentes_NaoMisturamOCache()
        {
            _api.BuscarEnderecoPorCEP("01001000").Returns(new EnderecoModel { Logradouro = "Praça da Sé" });
            _api.BuscarEnderecoPorCEP("01310100").Returns(new EnderecoModel { Logradouro = "Avenida Paulista" });

            var se = await _cachedApi.BuscarEnderecoPorCEP("01001000");
            var paulista = await _cachedApi.BuscarEnderecoPorCEP("01310100");

            se.Value.Logradouro.ShouldBe("Praça da Sé");
            paulista.Value.Logradouro.ShouldBe("Avenida Paulista");
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_CepNaoEncontrado_TambemFicaNoCache()
        {
            _api.BuscarEnderecoPorCEP("99999999").Returns(EnderecoErrors.CepNaoEncontrado);

            await _cachedApi.BuscarEnderecoPorCEP("99999999");
            var result = await _cachedApi.BuscarEnderecoPorCEP("99999999");

            result.Error.ShouldBe(EnderecoErrors.CepNaoEncontrado);
            await _api.Received(1).BuscarEnderecoPorCEP("99999999");
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoAApiExternaFalha_NaoGuardaEtentaDeNovo()
        {
            _api.BuscarEnderecoPorCEP("01001000").Returns(
                _ => throw new HttpRequestException("ViaCEP fora do ar"),
                _ => new EnderecoModel { Logradouro = "Praça da Sé" });

            await Should.ThrowAsync<HttpRequestException>(() => _cachedApi.BuscarEnderecoPorCEP("01001000"));
            var result = await _cachedApi.BuscarEnderecoPorCEP("01001000");

            result.Value.Logradouro.ShouldBe("Praça da Sé");
            await _api.Received(2).BuscarEnderecoPorCEP("01001000");
        }

        [Fact]
        public async Task BuscarPorEstadoECidade_NaSegundaVez_UsaOCache()
        {
            _api.BuscarPorEstadoECidade("SP", "São Paulo", "Paulista")
                .Returns(new List<EnderecoModel> { new() { CEP = "01310-100" } });

            await _cachedApi.BuscarPorEstadoECidade("SP", "São Paulo", "Paulista");
            var result = await _cachedApi.BuscarPorEstadoECidade("SP", "São Paulo", "Paulista");

            result.Value.Single().CEP.ShouldBe("01310-100");
            await _api.Received(1).BuscarPorEstadoECidade("SP", "São Paulo", "Paulista");
        }

        [Fact]
        public async Task BuscarCidadesPorUF_NaSegundaVez_UsaOCache()
        {
            _api.BuscarCidadesPorUF("AC").Returns(new List<CidadeModel> { new() { Nome = "Acrelândia" } });

            await _cachedApi.BuscarCidadesPorUF("AC");
            var result = await _cachedApi.BuscarCidadesPorUF("AC");

            result.Value.Single().Nome.ShouldBe("Acrelândia");
            await _api.Received(1).BuscarCidadesPorUF("AC");
        }
    }
}
