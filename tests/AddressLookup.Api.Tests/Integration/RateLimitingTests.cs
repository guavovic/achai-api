using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AddressLookup.Api.Common.Http;

namespace AddressLookup.Api.Tests.Integration;

public class RateLimitingTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public RateLimitingTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task AfterTheLimit_Returns429WithRetryAfterAndProblemDetails()
    {
        // CEP fora do formato: barrado na validação, sem depender das APIs externas.
        for (var i = 0; i < RateLimitingExtensions.PermitsPerMinute; i++)
        {
            var allowed = await _client.GetAsync("/buscar/123", _ct);
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        var rejected = await _client.GetAsync("/buscar/123", _ct);

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.ShouldNotBeNull();
        var problem = await rejected.Content.ReadFromJsonAsync<JsonElement>(_ct);
        problem.GetProperty("title").GetString().ShouldBe("Muitas requisições");
    }

    [Fact]
    public async Task Health_IsNotRateLimited()
    {
        for (var i = 0; i < RateLimitingExtensions.PermitsPerMinute + 10; i++)
        {
            var response = await _client.GetAsync("/health", _ct);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
