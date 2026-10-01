using System.Net;
using AddressLookup.Api.Common;
using AddressLookup.Api.Infrastructure.Ibge;
using AddressLookup.Api.Tests.Fakes;

namespace AddressLookup.Api.Tests.Unit.Infrastructure;

public class IbgeClientTests
{
    private readonly FakeHttpMessageHandler _ibge = new();
    private readonly IbgeClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public IbgeClientTests()
    {
        _client = new IbgeClient(_ibge.CreateClient("https://ibge.teste/api/v1/"));
    }

    [Fact]
    public async Task GetByStateAsync_WhenFound_ReturnsTheCities()
    {
        _ibge.RespondWith(HttpStatusCode.OK, ExternalResponses.IbgeAcreCities);

        var result = await _client.GetByStateAsync("AC", _ct);

        result.Value.Select(c => c.Name).ShouldBe(["Acrelândia", "Assis Brasil"]);
        _ibge.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/v1/localidades/estados/AC/municipios");
    }

    [Fact]
    public async Task GetByStateAsync_WhenIbgeReturnsEmptyList_ReturnsStateNotFound()
    {
        _ibge.RespondWith(HttpStatusCode.OK, ExternalResponses.EmptyList);

        var result = await _client.GetByStateAsync("XX", _ct);

        result.Error.ShouldBe(AddressErrors.StateNotFound);
    }
}
