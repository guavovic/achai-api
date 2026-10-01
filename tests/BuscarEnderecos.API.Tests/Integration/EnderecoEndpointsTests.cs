using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuscarEnderecos.API.Tests.Fakes;

namespace BuscarEnderecos.API.Tests.Integration
{
    public class EnderecoEndpointsTests : IDisposable
    {
        private readonly ApiFactory _factory = new();
        private readonly HttpClient _client;
        private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

        public EnderecoEndpointsTests()
        {
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Fact]
        public async Task BuscarPorCep_ComTraco_Devolve200ComOEndereco()
        {
            _factory.ViaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.EnderecoPracaDaSe);

            var response = await _client.GetAsync("/buscar/01001-000", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var endereco = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
            endereco.GetProperty("logradouro").GetString().ShouldBe("Praça da Sé");
            _factory.ViaCep.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/ws/01001000/json");
        }

        [Fact]
        public async Task BuscarPorCep_ComESemTraco_ChamaOViaCepUmaVezSo()
        {
            _factory.ViaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.EnderecoPracaDaSe);

            var primeira = await _client.GetAsync("/buscar/01001-000", _ct);
            var segunda = await _client.GetAsync("/buscar/01001000", _ct);

            primeira.StatusCode.ShouldBe(HttpStatusCode.OK);
            segunda.StatusCode.ShouldBe(HttpStatusCode.OK);
            (await segunda.Content.ReadAsStringAsync(_ct)).ShouldBe(await primeira.Content.ReadAsStringAsync(_ct));
            _factory.ViaCep.Requests.Count.ShouldBe(1);
        }

        [Fact]
        public async Task BuscarPorCep_Inexistente_Devolve404ComProblemDetails()
        {
            _factory.ViaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.CepInexistente);

            var response = await _client.GetAsync("/buscar/99999999", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
            problem.GetProperty("title").GetString().ShouldBe("Não encontrado");
            problem.GetProperty("code").GetString().ShouldBe("Endereco.CepNaoEncontrado");
        }

        [Theory]
        [InlineData("123")]
        [InlineData("0100100a")]
        [InlineData("01001-0000")]
        public async Task BuscarPorCep_ForaDoFormato_Devolve400SemChamarOViaCep(string cep)
        {
            var response = await _client.GetAsync($"/buscar/{cep}", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
            problem.GetProperty("title").GetString().ShouldBe("Um ou mais campos são inválidos.");
            problem.GetProperty("errors").TryGetProperty("cep", out _).ShouldBeTrue();
            _factory.ViaCep.Requests.ShouldBeEmpty();
        }

        [Fact]
        public async Task BuscarPorLogradouro_ComUfInexistente_Devolve400()
        {
            var response = await _client.GetAsync("/buscar/XX/Sao Paulo/Paulista", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
            problem.GetProperty("errors").TryGetProperty("uf", out _).ShouldBeTrue();
            _factory.ViaCep.Requests.ShouldBeEmpty();
        }

        [Fact]
        public async Task BuscarPorLogradouro_ComCidadeELogradouroCurtos_Devolve400ComOsDoisErros()
        {
            var response = await _client.GetAsync("/buscar/SP/Sa/Pa", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var erros = (await response.Content.ReadFromJsonAsync<JsonElement>(_ct)).GetProperty("errors");
            erros.TryGetProperty("cidade", out _).ShouldBeTrue();
            erros.TryGetProperty("logradouro", out _).ShouldBeTrue();
        }

        [Fact]
        public async Task BuscarPorLogradouro_SemResultado_Devolve200ComListaVazia()
        {
            _factory.ViaCep.RespondWith(HttpStatusCode.OK, RespostasExternas.ListaVazia);

            var response = await _client.GetAsync("/buscar/sp/Sao Paulo/Xyzqwk", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync(_ct)).ShouldBe("[]");
            _factory.ViaCep.Requests.Single().RequestUri!.AbsolutePath.ShouldStartWith("/ws/SP/");
        }

        [Fact]
        public async Task BuscarCidades_ComUfInexistente_Devolve400SemChamarOIbge()
        {
            var response = await _client.GetAsync("/buscar/cidades/XX", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            _factory.Ibge.Requests.ShouldBeEmpty();
        }

        [Fact]
        public async Task BuscarCidades_ComUfValida_Devolve200ComAsCidades()
        {
            _factory.Ibge.RespondWith(HttpStatusCode.OK, RespostasExternas.CidadesDoAcre);

            var response = await _client.GetAsync("/buscar/cidades/ac", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var cidades = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
            cidades.GetArrayLength().ShouldBe(2);
        }

        [Fact]
        public async Task QuandoOViaCepEstaFora_Devolve500SemVazarDetalhe()
        {
            _factory.ViaCep.ThrowOnRequest(new HttpRequestException("detalhe interno que não pode vazar"));

            var response = await _client.GetAsync("/buscar/01001000", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            var corpo = await response.Content.ReadAsStringAsync(_ct);
            corpo.ShouldContain("Erro interno");
            corpo.ShouldNotContain("detalhe interno");
        }

        [Fact]
        public async Task RotaInexistente_Devolve404NoMesmoFormato()
        {
            var response = await _client.GetAsync("/rota/inexistente", _ct);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
            problem.GetProperty("title").GetString().ShouldBe("Não encontrado");
        }
    }
}
