using System.Net;
using BuscarEnderecos.API.Errors;
using BuscarEnderecos.API.Rest;
using BuscarEnderecos.API.Tests.Fakes;

namespace BuscarEnderecos.API.Tests.Unit
{
    public class BuscarEnderecosApiRestTests
    {
        private readonly FakeHttpMessageHandler _viaCep = new();
        private readonly FakeHttpMessageHandler _ibge = new();
        private readonly BuscarEnderecosApiRest _apiRest;

        public BuscarEnderecosApiRestTests()
        {
            var factory = Substitute.For<IHttpClientFactory>();
            factory.CreateClient(BuscarEnderecosApiRest.ViaCepClient)
                .Returns(_ => new HttpClient(_viaCep) { BaseAddress = new Uri("https://viacep.teste/ws/") });
            factory.CreateClient(BuscarEnderecosApiRest.IbgeClient)
                .Returns(_ => new HttpClient(_ibge) { BaseAddress = new Uri("https://ibge.teste/api/v1/") });

            _apiRest = new BuscarEnderecosApiRest(factory);
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoOViaCepAcha_DevolveOEndereco()
        {
            _viaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.EnderecoPracaDaSe);

            var result = await _apiRest.BuscarEnderecoPorCEP("01001000");

            result.IsSuccess.ShouldBeTrue();
            result.Value.Logradouro.ShouldBe("Praça da Sé");
            _viaCep.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/ws/01001000/json");
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoOViaCepDevolveErroTrue_DevolveCepNaoEncontrado()
        {
            _viaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.CepInexistente);

            var result = await _apiRest.BuscarEnderecoPorCEP("99999999");

            result.Error.ShouldBe(EnderecoErrors.CepNaoEncontrado);
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoOViaCepDevolve400EmHtml_DevolveCepInvalido()
        {
            _viaCep.RespondWith(HttpStatusCode.BadRequest, RespostasExternas.PaginaDeErro400, "text/html");

            var result = await _apiRest.BuscarEnderecoPorCEP("123");

            result.Error.ShouldBe(EnderecoErrors.CepInvalido);
        }

        [Fact]
        public async Task BuscarEnderecoPorCEP_QuandoOViaCepCai_LancaExcecao()
        {
            _viaCep.RespondWith(HttpStatusCode.InternalServerError, "");

            await Should.ThrowAsync<HttpRequestException>(() => _apiRest.BuscarEnderecoPorCEP("01001000"));
        }

        [Fact]
        public async Task BuscarPorEstadoECidade_QuandoNaoAcha_DevolveListaVazia()
        {
            _viaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.ListaVazia);

            var result = await _apiRest.BuscarPorEstadoECidade("SP", "São Paulo", "Xyzqwk");

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeEmpty();
        }

        [Fact]
        public async Task BuscarPorEstadoECidade_QuandoAcha_DevolveOsEnderecos()
        {
            _viaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.ListaComUmEndereco);

            var result = await _apiRest.BuscarPorEstadoECidade("SP", "São Paulo", "Praça da Sé");

            result.Value.Single().CEP.ShouldBe("01001-000");
        }

        [Fact]
        public async Task BuscarPorEstadoECidade_QuandoOViaCepDevolve400_DevolveBuscaInvalida()
        {
            _viaCep.RespondWith(HttpStatusCode.BadRequest, RespostasExternas.PaginaDeErro400, "text/html");

            var result = await _apiRest.BuscarPorEstadoECidade("SP", "Sa", "Pa");

            result.Error.ShouldBe(EnderecoErrors.BuscaInvalida);
        }

        [Fact]
        public async Task BuscarCidadesPorUF_QuandoOIbgeAcha_DevolveAsCidades()
        {
            _ibge.RespondWith(HttpStatusCode.OK, RespostasExternas.CidadesDoAcre);

            var result = await _apiRest.BuscarCidadesPorUF("AC");

            result.Value.Select(c => c.Nome).ShouldBe(["Acrelândia", "Assis Brasil"]);
            _ibge.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/v1/localidades/estados/AC/municipios");
        }

        [Fact]
        public async Task BuscarCidadesPorUF_QuandoOIbgeDevolveListaVazia_DevolveUfNaoEncontrada()
        {
            _ibge.RespondWith(HttpStatusCode.OK, RespostasExternas.ListaVazia);

            var result = await _apiRest.BuscarCidadesPorUF("XX");

            result.Error.ShouldBe(EnderecoErrors.UfNaoEncontrada);
        }
    }
}
