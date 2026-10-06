using System.Net;
using System.Text.Json;
using Achai.Api.Common.Http;

namespace Achai.Api.Tests.Integration;

public class DocumentationTests : IDisposable
{
    private readonly ApiFactory _factory = new("Production");
    private readonly HttpClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public DocumentationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task OpenApiDocument_DescribesEveryEndpoint()
    {
        var response = await _client.GetAsync("/openapi/v1.json", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(_ct));
        var root = document.RootElement;
        root.GetProperty("info").GetProperty("title").GetString().ShouldBe("achaí-API");

        var paths = root.GetProperty("paths");
        paths.EnumerateObject().Select(path => path.Name).ShouldBe(
            ["/buscar/{cep}", "/buscar/{uf}/{cidade}/{logradouro}", "/buscar/cidades/{uf}"],
            ignoreOrder: true);

        var zipCodeResponses = paths.GetProperty("/buscar/{cep}").GetProperty("get").GetProperty("responses");
        zipCodeResponses.EnumerateObject().Select(status => status.Name).ShouldBe(["200", "400", "404", "429"], ignoreOrder: true);
    }

    [Fact]
    public async Task ScalarReference_IsPublished()
    {
        var response = await _client.GetAsync($"{ApiDocumentationExtensions.DocsPath}/", _ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/html");
        (await response.Content.ReadAsStringAsync(_ct)).ShouldContain("<title>achaí-API</title>");
    }
}
