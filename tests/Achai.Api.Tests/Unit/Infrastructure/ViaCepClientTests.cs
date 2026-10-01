using System.Net;
using Achai.Api.Common;
using Achai.Api.Infrastructure.ViaCep;
using Achai.Api.Tests.Fakes;

namespace Achai.Api.Tests.Unit.Infrastructure;

public class ViaCepClientTests
{
    private readonly FakeHttpMessageHandler _viaCep = new();
    private readonly ViaCepClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public ViaCepClientTests()
    {
        _client = new ViaCepClient(_viaCep.CreateClient("https://viacep.teste/ws/"));
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenFound_ReturnsTheAddress()
    {
        _viaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepPracaDaSe);

        var result = await _client.GetByZipCodeAsync("01001000", _ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Street.ShouldBe("Praça da Sé");
        _viaCep.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/ws/01001000/json");
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenViaCepReturnsErroTrue_ReturnsZipCodeNotFound()
    {
        _viaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepNotFound);

        var result = await _client.GetByZipCodeAsync("99999999", _ct);

        result.Error.ShouldBe(AddressErrors.ZipCodeNotFound);
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenViaCepReturns400Html_ReturnsInvalidZipCode()
    {
        _viaCep.RespondWith(HttpStatusCode.BadRequest, ExternalResponses.ViaCepBadRequestPage, "text/html");

        var result = await _client.GetByZipCodeAsync("123", _ct);

        result.Error.ShouldBe(AddressErrors.InvalidZipCode);
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenViaCepIsDown_Throws()
    {
        _viaCep.RespondWith(HttpStatusCode.InternalServerError, "");

        await Should.ThrowAsync<HttpRequestException>(() => _client.GetByZipCodeAsync("01001000", _ct));
    }

    [Fact]
    public async Task SearchByStreetAsync_WhenNothingIsFound_ReturnsEmptyList()
    {
        _viaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.EmptyList);

        var result = await _client.SearchByStreetAsync("SP", "São Paulo", "Xyzqwk", _ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task SearchByStreetAsync_WhenFound_ReturnsTheAddresses()
    {
        _viaCep.RespondWith(HttpStatusCode.OK, ExternalResponses.ViaCepListWithPracaDaSe);

        var result = await _client.SearchByStreetAsync("SP", "São Paulo", "Praça da Sé", _ct);

        result.Value.Single().ZipCode.ShouldBe("01001-000");
    }

    [Fact]
    public async Task SearchByStreetAsync_WhenViaCepReturns400_ReturnsInvalidStreetSearch()
    {
        _viaCep.RespondWith(HttpStatusCode.BadRequest, ExternalResponses.ViaCepBadRequestPage, "text/html");

        var result = await _client.SearchByStreetAsync("SP", "Sa", "Pa", _ct);

        result.Error.ShouldBe(AddressErrors.InvalidStreetSearch);
    }
}
