using System.Net;
using Achai.Api.Tests.Fakes;
using Achai.Client;
using Achai.Client.Models;

namespace Achai.Api.Tests.Integration;

public class DotnetClientTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _http;
    private readonly AchaiApiClient _achai;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public DotnetClientTests()
    {
        _http = _factory.CreateClient();
        _achai = AchaiClient.Create(_http.BaseAddress!.ToString(), _http);
    }

    public void Dispose()
    {
        _http.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task SearchesAZipCodeThroughTheGeneratedClient()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);

        var address = await _achai.Buscar["01001000"].GetAsync(cancellationToken: _ct);

        address.ShouldNotBeNull();
        address.Logradouro.ShouldBe("Praça da Sé");
        address.EnderecoFormatado.ShouldBe("Praça da Sé, lado ímpar - Sé, São Paulo/SP, CEP 01001-000");
    }

    [Fact]
    public async Task SendsABatchThroughTheGeneratedClient()
    {
        _factory.ViaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);

        var items = await _achai.Buscar.Lote.PostAsync(new BatchRequest { Ceps = ["01001000", "123"] }, cancellationToken: _ct);

        items.ShouldNotBeNull();
        items.Select(item => item.Status).ShouldBe(["encontrado", "invalido"]);
    }
}
