using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Achai.Api.Tests.Integration;

public class SecurityHeadersTests : IDisposable
{
    private readonly ApiFactory _factory = new("Production");
    private readonly HttpClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public SecurityHeadersTests()
    {
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://achai-api.onrender.com"),
            AllowAutoRedirect = false
        });
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Theory]
    [InlineData("/health", HttpStatusCode.OK)]
    [InlineData("/buscar/123", HttpStatusCode.BadRequest)]
    [InlineData("/rota-que-nao-existe", HttpStatusCode.NotFound)]
    public async Task EveryResponse_HasTheSecurityHeaders(string path, HttpStatusCode expectedStatus)
    {
        using var response = await _client.GetAsync(path, _ct);

        response.StatusCode.ShouldBe(expectedStatus);
        HeaderOf(response, "X-Content-Type-Options").ShouldBe("nosniff");
        HeaderOf(response, "X-Frame-Options").ShouldBe("DENY");
        HeaderOf(response, "Referrer-Policy").ShouldBe("no-referrer");
        HeaderOf(response, "Content-Security-Policy").ShouldBe("default-src 'none'; frame-ancestors 'none'");
    }

    [Fact]
    public async Task InProduction_HttpsResponsesHaveHsts()
    {
        using var response = await _client.GetAsync("/health", _ct);

        HeaderOf(response, "Strict-Transport-Security").ShouldBe("max-age=31536000");
    }

    [Fact]
    public async Task InDevelopment_ThereIsNoHsts()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://achai-api.onrender.com")
        });

        using var response = await client.GetAsync("/health", _ct);

        HeaderOf(response, "Strict-Transport-Security").ShouldBeNull();
    }

    private static string? HeaderOf(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) || response.Content.Headers.TryGetValues(name, out values)
            ? values.Single()
            : null;
}
