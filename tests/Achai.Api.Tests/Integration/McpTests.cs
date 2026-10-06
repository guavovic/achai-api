using System.Net;
using System.Text;
using System.Text.Json;
using Achai.Api.Tests.Fakes;

namespace Achai.Api.Tests.Integration;

public class McpTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public McpTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task ListsTheAddressTools()
    {
        var result = await CallAsync("tools/list", new { });

        result.GetProperty("tools").EnumerateArray().Select(tool => tool.GetProperty("name").GetString())
            .ShouldBe(["buscar_cep", "buscar_logradouro", "listar_cidades", "distancia_entre_ceps"], ignoreOrder: true);
    }

    [Fact]
    public async Task SearchZipCode_ReturnsTheAddress()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);

        var result = await CallAsync("tools/call", new { name = "buscar_cep", arguments = new { cep = "01001-000" } });

        (result.TryGetProperty("isError", out var isError) && isError.GetBoolean()).ShouldBeFalse();
        var text = result.GetProperty("content")[0].GetProperty("text").GetString()!;
        text.ShouldContain("\"logradouro\":\"Praça da Sé\"");
    }

    [Fact]
    public async Task SearchZipCode_WithInvalidZipCode_ReturnsAReadableError()
    {
        var result = await CallAsync("tools/call", new { name = "buscar_cep", arguments = new { cep = "123" } });

        result.GetProperty("isError").GetBoolean().ShouldBeTrue();
        result.GetProperty("content")[0].GetProperty("text").GetString()!.ShouldContain("8 dígitos");
        _factory.ViaCep.Requests.ShouldBeEmpty();
    }

    private async Task<JsonElement> CallAsync(string method, object parameters)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { jsonrpc = "2.0", id = 1, method, @params = parameters }),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");

        var response = await _client.SendAsync(request, _ct);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync(_ct);
        var json = body.Split('\n').Where(line => line.StartsWith("data: ")).Select(line => line["data: ".Length..]).Single();
        return JsonDocument.Parse(json).RootElement.GetProperty("result").Clone();
    }
}
