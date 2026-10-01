using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Achai.Api.Common.Http;

namespace Achai.Api.Tests.Integration;

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
    public async Task WithClientIpHeader_LimitsByThatHeaderEvenWhenTheProxyChainChanges()
    {
        using var factory = new ApiFactory(settings: new Dictionary<string, string>
        {
            ["ForwardedHeaders:ClientIpHeader"] = "True-Client-IP"
        });
        using var client = factory.CreateClient();

        // Como no Render: a última entrada do X-Forwarded-For é o balanceador, que muda a cada requisição.
        for (var i = 0; i < RateLimitingExtensions.PermitsPerMinute; i++)
        {
            var allowed = await SendThroughProxiesAsync(client, clientIp: "203.0.113.7", lastProxy: $"10.0.0.{i}");
            allowed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        var rejected = await SendThroughProxiesAsync(client, clientIp: "203.0.113.7", lastProxy: "10.0.0.250");
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var otherClient = await SendThroughProxiesAsync(client, clientIp: "198.51.100.20", lastProxy: "10.0.0.251");
        otherClient.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
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

    private async Task<HttpResponseMessage> SendThroughProxiesAsync(HttpClient client, string clientIp, string lastProxy)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/buscar/123");
        request.Headers.Add("True-Client-IP", clientIp);
        request.Headers.Add("X-Forwarded-For", $"{clientIp}, 172.71.195.123, {lastProxy}");

        return await client.SendAsync(request, _ct);
    }
}
