using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Achai.Api.Features.Addresses;
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
    public async Task SearchByStreet_WhenOnlyAVariationFinds_ReturnsItAndSaysWhich()
    {
        _factory.ViaCep.RespondWith(request => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                Uri.UnescapeDataString(request.RequestUri!.AbsolutePath).Contains("15 de Novembro")
                    ? ExternalResponses.ViaCepListWithPracaDaSe
                    : ExternalResponses.EmptyList,
                Encoding.UTF8,
                "application/json")
        });

        var response = await _client.GetAsync("/buscar/SC/Blumenau/XV de Novembro", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<JsonElement>(_ct)).GetArrayLength().ShouldBe(1);
        response.Headers.GetValues(SearchAddressesByStreet.SearchedStreetHeader).Single().ShouldBe("15%20de%20Novembro");
        _factory.ViaCep.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task SearchByStreet_WhenTheNameFinds_DoesNotTryVariationsNorSetTheHeader()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepListWithPracaDaSe);

        var response = await _client.GetAsync("/buscar/SP/Sao Paulo/Av. Paulista", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.Contains(SearchAddressesByStreet.SearchedStreetHeader).ShouldBeFalse();
        Uri.UnescapeDataString(_factory.ViaCep.Requests.Single().RequestUri!.AbsolutePath).ShouldEndWith("/Av Paulista/json");
    }

    [Fact]
    public async Task Batch_ReturnsOneItemPerZipCodeInTheSameOrder()
    {
        _factory.ViaCep.RespondWith(request => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                request.RequestUri!.AbsolutePath.Contains("01001000")
                    ? ExternalResponses.ViaCepPracaDaSe
                    : ExternalResponses.ViaCepNotFound,
                Encoding.UTF8,
                "application/json")
        });

        var response = await _client.PostAsJsonAsync("/buscar/lote", new { ceps = new[] { "01001-000", "99999999", "123" } }, _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = (await response.Content.ReadFromJsonAsync<JsonElement>(_ct)).EnumerateArray().ToList();
        items.Select(item => item.GetProperty("status").GetString()).ShouldBe(["encontrado", "nao_encontrado", "invalido"]);
        items[0].GetProperty("cep").GetString().ShouldBe("01001-000");
        items[0].GetProperty("endereco").GetProperty("logradouro").GetString().ShouldBe("Praça da Sé");
        items[1].GetProperty("endereco").ValueKind.ShouldBe(JsonValueKind.Null);
        _factory.ViaCep.Requests.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task Batch_WithoutZipCodesOrWithTooMany_Returns400(int count)
    {
        var ceps = Enumerable.Repeat("01001000", count).ToArray();

        var response = await _client.PostAsJsonAsync("/buscar/lote", new { ceps }, _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        _factory.ViaCep.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Consensus_WhenBothSourcesAgree_SaysSo()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);
        _factory.BrasilApi.RespondWith(HttpStatusCode.OK, ExternalResponses.BrasilApiPracaDaSe);

        var response = await _client.GetAsync("/buscar/01001-000/consenso", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        body.GetProperty("concordam").GetBoolean().ShouldBeTrue();
        body.GetProperty("divergencias").GetArrayLength().ShouldBe(0);
        body.GetProperty("fontes").EnumerateArray().Select(source => source.GetProperty("status").GetString())
            .ShouldBe(["encontrado", "encontrado"]);
    }

    [Fact]
    public async Task Consensus_WhenOnlyOneSourceFinds_ReportsTheDifference()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepNotFound);
        _factory.BrasilApi.RespondWith(HttpStatusCode.OK, ExternalResponses.BrasilApiPracaDaSe);

        var response = await _client.GetAsync("/buscar/99999999/consenso", _ct);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        body.GetProperty("concordam").GetBoolean().ShouldBeFalse();
        body.GetProperty("divergencias").EnumerateArray().Select(field => field.GetString()).ShouldBe(["encontrado"]);
        body.GetProperty("fontes")[0].GetProperty("status").GetString().ShouldBe("nao_encontrado");
    }

    [Fact]
    public async Task Consensus_ComparesTheFieldsIgnoringAccents()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);
        _factory.BrasilApi.RespondWith(HttpStatusCode.OK, ExternalResponses.BrasilApiPracaDaSe
            .Replace("Praça da Sé", "PRACA DA SE").Replace("\"Sé\"", "\"Centro\""));

        var body = await (await _client.GetAsync("/buscar/01001000/consenso", _ct)).Content.ReadFromJsonAsync<JsonElement>(_ct);

        body.GetProperty("divergencias").EnumerateArray().Select(field => field.GetString()).ShouldBe(["bairro"]);
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
