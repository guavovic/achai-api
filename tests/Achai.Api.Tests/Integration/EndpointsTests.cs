using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Achai.Api.Tests.Fakes;

namespace Achai.Api.Tests.Integration;

public class EndpointsTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public EndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetByZipCode_WithHyphen_Returns200WithTheAddress()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);

        var response = await _client.GetAsync("/buscar/01001-000", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var address = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        address.GetProperty("logradouro").GetString().ShouldBe("Praça da Sé");
        address.GetProperty("tipoLogradouro").GetString().ShouldBe("Praça");
        address.GetProperty("nomeLogradouro").GetString().ShouldBe("da Sé");
        address.GetProperty("enderecoFormatado").GetString().ShouldBe("Praça da Sé, lado ímpar - Sé, São Paulo/SP, CEP 01001-000");
        _factory.ViaCep.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/ws/01001000/json");
    }

    [Fact]
    public async Task GetByZipCode_WithAndWithoutHyphen_CallsViaCepOnlyOnce()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);

        var first = await _client.GetAsync("/buscar/01001-000", _ct);
        var second = await _client.GetAsync("/buscar/01001000", _ct);

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await second.Content.ReadAsStringAsync(_ct)).ShouldBe(await first.Content.ReadAsStringAsync(_ct));
        _factory.ViaCep.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetByZipCode_WhenZipCodeDoesNotExist_Returns404WithProblemDetails()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepNotFound);

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
    public async Task GetByZipCode_WithInvalidFormat_Returns400WithoutCallingViaCep(string zipCode)
    {
        var response = await _client.GetAsync($"/buscar/{zipCode}", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        problem.GetProperty("title").GetString().ShouldBe("Um ou mais campos são inválidos.");
        problem.GetProperty("errors").TryGetProperty("cep", out _).ShouldBeTrue();
        _factory.ViaCep.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task SearchByStreet_WithUnknownState_Returns400()
    {
        var response = await _client.GetAsync("/buscar/XX/Sao Paulo/Paulista", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        problem.GetProperty("errors").TryGetProperty("uf", out _).ShouldBeTrue();
        _factory.ViaCep.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task SearchByStreet_WithShortCityAndStreet_Returns400WithBothErrors()
    {
        var response = await _client.GetAsync("/buscar/SP/Sa/Pa", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>(_ct)).GetProperty("errors");
        errors.TryGetProperty("cidade", out _).ShouldBeTrue();
        errors.TryGetProperty("logradouro", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task SearchByStreet_WithNoResults_Returns200WithEmptyList()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.EmptyList);

        var response = await _client.GetAsync("/buscar/sp/Sao Paulo/Xyzqwk", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(_ct)).ShouldBe("[]");
        _factory.ViaCep.Requests.Single().RequestUri!.AbsolutePath.ShouldStartWith("/ws/SP/");
    }

    [Fact]
    public async Task GetCities_WithUnknownState_Returns400WithoutCallingIbge()
    {
        var response = await _client.GetAsync("/buscar/cidades/XX", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        _factory.Ibge.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCities_WithValidState_Returns200WithTheCities()
    {
        _factory.Ibge.RespondWith(HttpStatusCode.OK, ExternalResponses.IbgeAcreCities);

        var response = await _client.GetAsync("/buscar/cidades/ac", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cities = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        cities.GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public async Task WhenViaCepAndBrasilApiAreDown_Returns500WithoutLeakingDetails()
    {
        _factory.ViaCep.ThrowOnRequest(new HttpRequestException("detalhe interno que não pode vazar"));
        _factory.BrasilApi.ThrowOnRequest(new HttpRequestException("outro detalhe interno"));

        var response = await _client.GetAsync("/buscar/01001000", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadAsStringAsync(_ct);
        body.ShouldContain("Erro interno");
        body.ShouldNotContain("detalhe interno");
    }

    [Fact]
    public async Task WhenViaCepIsDown_RetriesAndThenUsesBrasilApi()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.ServiceUnavailable, "");
        _factory.BrasilApi.RespondWith(HttpStatusCode.OK, ExternalResponses.BrasilApiPracaDaSe);

        var response = await _client.GetAsync("/buscar/01001-000", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var address = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        address.GetProperty("cep").GetString().ShouldBe("01001-000");
        address.GetProperty("estado").GetString().ShouldBe("São Paulo");
        address.GetProperty("regiao").GetString().ShouldBe("Sudeste");
        _factory.ViaCep.Requests.Count.ShouldBe(3);
        _factory.BrasilApi.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task WhenViaCepSaysZipCodeDoesNotExist_DoesNotCallBrasilApi()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepNotFound);

        var response = await _client.GetAsync("/buscar/99999999", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        _factory.BrasilApi.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task WhenViaCepFailsOnce_RetrySucceedsWithoutBrasilApi()
    {
        var attempts = 0;
        _factory.ViaCep.RespondWith(_ => ++attempts == 1
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ExternalResponses.ViaCepPracaDaSe) });

        var response = await _client.GetAsync("/buscar/01001000", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _factory.ViaCep.Requests.Count.ShouldBe(2);
        _factory.BrasilApi.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task UnknownRoute_Returns404InTheSameFormat()
    {
        var response = await _client.GetAsync("/rota/inexistente", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        problem.GetProperty("title").GetString().ShouldBe("Não encontrado");
    }
}
