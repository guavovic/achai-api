using System.Net;
using Achai.Api.Common;
using Achai.Api.Infrastructure.BrasilApi;
using Achai.Api.Tests.Fakes;

namespace Achai.Api.Tests.Unit.Infrastructure;

public class BrasilApiClientTests
{
    private readonly FakeHttpMessageHandler _brasilApi = new();
    private readonly BrasilApiClient _client;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public BrasilApiClientTests()
    {
        _client = new BrasilApiClient(_brasilApi.CreateClient("https://brasilapi.teste/api/"));
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenFound_ConvertsToTheViaCepFormat()
    {
        _brasilApi.RespondWith(HttpStatusCode.OK, ExternalResponses.BrasilApiPracaDaSe);

        var result = await _client.GetByZipCodeAsync("01001000", _ct);

        result.Value.ShouldSatisfyAllConditions(
            a => a.ZipCode.ShouldBe("01001-000"),
            a => a.Street.ShouldBe("Praça da Sé"),
            a => a.Neighborhood.ShouldBe("Sé"),
            a => a.City.ShouldBe("São Paulo"),
            a => a.State.ShouldBe("SP"),
            a => a.StateName.ShouldBe("São Paulo"),
            a => a.Region.ShouldBe("Sudeste"),
            a => a.Complement.ShouldBe(""),
            a => a.Unit.ShouldBe(""));
        _brasilApi.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/cep/v2/01001000");
    }

    [Fact]
    public async Task GetByZipCodeAsync_When404_ReturnsZipCodeNotFound()
    {
        _brasilApi.RespondWith(HttpStatusCode.NotFound, ExternalResponses.BrasilApiNotFound);

        var result = await _client.GetByZipCodeAsync("00000000", _ct);

        result.Error.ShouldBe(AddressErrors.ZipCodeNotFound);
    }

    [Fact]
    public async Task GetByZipCodeAsync_When400_ReturnsInvalidZipCode()
    {
        _brasilApi.RespondWith(HttpStatusCode.BadRequest, ExternalResponses.BrasilApiBadRequest);

        var result = await _client.GetByZipCodeAsync("0100100", _ct);

        result.Error.ShouldBe(AddressErrors.InvalidZipCode);
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenDown_Throws()
    {
        _brasilApi.RespondWith(HttpStatusCode.ServiceUnavailable, "");

        await Should.ThrowAsync<HttpRequestException>(() => _client.GetByZipCodeAsync("01001000", _ct));
    }
}
