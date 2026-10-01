using System.Text.Json;
using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Models;
using BuscarEnderecos.API.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class FallbackApiTests
    {
        private readonly IApi _viaCep = Substitute.For<IApi>();
        private readonly ICepReserva _reserva = Substitute.For<ICepReserva>();
        private readonly FallbackApi _fallbackApi;

        public FallbackApiTests()
        {
            _fallbackApi = new FallbackApi(_viaCep, _reserva, NullLogger<FallbackApi>.Instance);
            _reserva.BuscarEnderecoPorCEP(Arg.Any<string>()).Returns(new EnderecoModel { Logradouro = "Da reserva" });
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoOViaCepResponde_NaoUsaAReserva()
        {
            _viaCep.BuscarEnderecoPorCEP("01001000").Returns(new EnderecoModel { Logradouro = "Do ViaCEP" });

            var result = await _fallbackApi.BuscarEnderecoPorCEP("01001000");

            result.Value.Logradouro.ShouldBe("Do ViaCEP");
            await _reserva.DidNotReceive().BuscarEnderecoPorCEP(Arg.Any<string>());
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoOViaCepDizQueNaoExiste_NaoUsaAReserva()
        {
            _viaCep.BuscarEnderecoPorCEP("99999999").Returns(EnderecoErrors.CepNaoEncontrado);

            var result = await _fallbackApi.BuscarEnderecoPorCEP("99999999");

            result.Error.ShouldBe(EnderecoErrors.CepNaoEncontrado);
            await _reserva.DidNotReceive().BuscarEnderecoPorCEP(Arg.Any<string>());
        }

        public static TheoryData<Exception> FalhasDoViaCep => new()
        {
            new HttpRequestException("fora do ar"),
            new TimeoutRejectedException("demorou demais"),
            new BrokenCircuitException("circuito aberto"),
            new JsonException("resposta quebrada")
        };

        [Theory]
        [MemberData(nameof(FalhasDoViaCep))]
        public async Task BuscarEnderecoPorCEP_QuandoOViaCepFalha_UsaAReserva(Exception falha)
        {
            _viaCep.BuscarEnderecoPorCEP("01001000").ThrowsAsync(falha);

            var result = await _fallbackApi.BuscarEnderecoPorCEP("01001000");

            result.Value.Logradouro.ShouldBe("Da reserva");
            await _reserva.Received(1).BuscarEnderecoPorCEP("01001000");
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoOErroEDeProgramacao_NaoEscondeComAReserva()
        {
            _viaCep.BuscarEnderecoPorCEP("01001000").ThrowsAsync(new InvalidOperationException("bug"));

            await Should.ThrowAsync<InvalidOperationException>(() => _fallbackApi.BuscarEnderecoPorCEP("01001000"));
            await _reserva.DidNotReceive().BuscarEnderecoPorCEP(Arg.Any<string>());
        }

        [Fact]
        public async Task BuscarPorEstadoECidade_RepassaParaOViaCep()
        {
            _viaCep.BuscarPorEstadoECidade("SP", "São Paulo", "Paulista").Returns(new List<EnderecoModel>());

            await _fallbackApi.BuscarPorEstadoECidade("SP", "São Paulo", "Paulista");

            await _viaCep.Received(1).BuscarPorEstadoECidade("SP", "São Paulo", "Paulista");
        }

        [Fact]
        public async Task BuscarCidadesPorUF_RepassaParaOIbge()
        {
            _viaCep.BuscarCidadesPorUF("AC").Returns(new List<CidadeModel>());

            await _fallbackApi.BuscarCidadesPorUF("AC");

            await _viaCep.Received(1).BuscarCidadesPorUF("AC");
        }
    }
}
