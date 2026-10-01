using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Achai.Api.Tests.Fakes;

namespace Achai.Api.Tests.Integration;

public class HealthEndpointsTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public HealthEndpointsTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Health_ReturnsHealthyWithoutCallingExternalApis()
    {
        var response = await _client.GetAsync("/health", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(_ct)).ShouldBe("Healthy");
        _factory.ViaCep.Requests.ShouldBeEmpty();
        _factory.Ibge.Requests.ShouldBeEmpty();
        _factory.BrasilApi.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Ready_WhenEverySourceAnswers_ReturnsHealthy()
    {
        RespondFromEverySource();

        var report = await GetReadyReportAsync();

        report.GetProperty("status").GetString().ShouldBe("Healthy");
        StatusOf(report, "viacep").ShouldBe("Healthy");
        StatusOf(report, "brasilapi").ShouldBe("Healthy");
        StatusOf(report, "ibge").ShouldBe("Healthy");
    }

    [Fact]
    public async Task Ready_WhenViaCepIsDown_ReturnsDegradedWithoutLeakingTheError()
    {
        RespondFromEverySource();
        _factory.ViaCep.ThrowOnRequest(new HttpRequestException("detalhe interno que não pode vazar"));

        var response = await _client.GetAsync("/health/ready", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(_ct)).ShouldNotContain("detalhe interno");
        var report = await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
        report.GetProperty("status").GetString().ShouldBe("Degraded");
        StatusOf(report, "viacep").ShouldBe("Degraded");
        StatusOf(report, "brasilapi").ShouldBe("Healthy");
    }

    private void RespondFromEverySource()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);
        _factory.BrasilApi.RespondWith(HttpStatusCode.OK, ExternalResponses.BrasilApiPracaDaSe);
        _factory.Ibge.RespondWith(HttpStatusCode.OK, ExternalResponses.IbgeAcreCities);
    }

    private async Task<JsonElement> GetReadyReportAsync()
    {
        var response = await _client.GetAsync("/health/ready", _ct);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(_ct);
    }

    private static string? StatusOf(JsonElement report, string check) =>
        report.GetProperty("checks").EnumerateArray()
            .Single(entry => entry.GetProperty("name").GetString() == check)
            .GetProperty("status").GetString();
}
