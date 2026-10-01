namespace AddressLookup.Api.Tests.Integration;

public class CorsTests
{
    private const string FrontOrigin = "https://consultar-enderecos-api-ten.vercel.app";
    private const string OtherOrigin = "https://site-qualquer.com";

    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    [Fact]
    public async Task InProduction_TheFrontOriginIsAllowed()
    {
        var allowOrigin = await GetAllowOriginAsync("Production", FrontOrigin);

        allowOrigin.ShouldBe(FrontOrigin);
    }

    [Fact]
    public async Task InProduction_OtherOriginsAreNotAllowed()
    {
        var allowOrigin = await GetAllowOriginAsync("Production", OtherOrigin);

        allowOrigin.ShouldBeNull();
    }

    [Fact]
    public async Task InDevelopment_AnyOriginIsAllowed()
    {
        var allowOrigin = await GetAllowOriginAsync("Development", OtherOrigin);

        allowOrigin.ShouldBe("*");
    }

    private async Task<string?> GetAllowOriginAsync(string environment, string origin)
    {
        using var factory = new ApiFactory(environment);
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", origin);

        using var response = await client.SendAsync(request, _ct);

        return response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values)
            ? values.Single()
            : null;
    }
}
