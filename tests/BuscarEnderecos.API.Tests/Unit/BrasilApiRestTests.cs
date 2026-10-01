using System.Net;
using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Rest;
using BuscarEnderecos.API.Tests.Fakes;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class BrasilApiRestTests
    {
        private readonly FakeHttpMessageHandler _brasilApi = new();
        private readonly BrasilApiRest _apiRest;

        public BrasilApiRestTests()
        {
            var factory = Substitute.For<IHttpClientFactory>();
            factory.CreateClient(BrasilApiRest.BrasilApiClient)
                .Returns(_ => new HttpClient(_brasilApi) { BaseAddress = new Uri("https://brasilapi.teste/api/") });

            _apiRest = new BrasilApiRest(factory);
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoAcha_ConverteParaOFormatoDoViaCep()
        {
            _brasilApi.RespondWith(HttpStatusCode.OK, RespostasExternas.BrasilApiPracaDaSe);

            var result = await _apiRest.BuscarEnderecoPorCEP("01001000");

            result.Value.ShouldSatisfyAllConditions(
                e => e.CEP.ShouldBe("01001-000"),
                e => e.Logradouro.ShouldBe("Praça da Sé"),
                e => e.Bairro.ShouldBe("Sé"),
                e => e.Localidade.ShouldBe("São Paulo"),
                e => e.UF.ShouldBe("SP"),
                e => e.Estado.ShouldBe("São Paulo"),
                e => e.Regiao.ShouldBe("Sudeste"),
                e => e.Complemento.ShouldBe(""),
                e => e.Unidade.ShouldBe(""));
            _brasilApi.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/cep/v2/01001000");
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoDevolve404_DevolveCepNaoEncontrado()
        {
            _brasilApi.RespondWith(HttpStatusCode.NotFound, RespostasExternas.BrasilApiCepNaoEncontrado);

            var result = await _apiRest.BuscarEnderecoPorCEP("00000000");

            result.Error.ShouldBe(EnderecoErrors.CepNaoEncontrado);
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoDevolve400_DevolveCepInvalido()
        {
            _brasilApi.RespondWith(HttpStatusCode.BadRequest, RespostasExternas.BrasilApiCepInvalido);

            var result = await _apiRest.BuscarEnderecoPorCEP("0100100");

            result.Error.ShouldBe(EnderecoErrors.CepInvalido);
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoCai_LancaExcecao()
        {
            _brasilApi.RespondWith(HttpStatusCode.ServiceUnavailable, "");

            await Should.ThrowAsync<HttpRequestException>(() => _apiRest.BuscarEnderecoPorCEP("01001000"));
        }
    }
}
