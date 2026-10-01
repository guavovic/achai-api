using System.Text.Json;
using AddressLookup.Api.Common;
using AddressLookup.Api.Infrastructure;
using AddressLookup.Api.Infrastructure.Fallback;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace AddressLookup.Api.Tests.Unit.Infrastructure;

public class FallbackAddressProviderTests
{
    private static readonly Address FromViaCep = new("01001-000", "Do ViaCEP", null, null, null, null, "SP", null, null);
    private static readonly Address FromFallback = new("01001-000", "Da reserva", null, null, null, null, "SP", null, null);

    private readonly IAddressProvider _primary = Substitute.For<IAddressProvider>();
    private readonly IZipCodeProvider _fallback = Substitute.For<IZipCodeProvider>();
    private readonly FallbackAddressProvider _provider;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public FallbackAddressProviderTests()
    {
        _provider = new FallbackAddressProvider(_primary, _fallback, NullLogger<FallbackAddressProvider>.Instance);
        _fallback.GetByZipCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(FromFallback);
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenPrimaryAnswers_DoesNotUseTheFallback()
    {
        _primary.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).Returns(FromViaCep);

        var result = await _provider.GetByZipCodeAsync("01001000", _ct);

        result.Value.Street.ShouldBe("Do ViaCEP");
        await _fallback.DidNotReceive().GetByZipCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenPrimarySaysNotFound_DoesNotUseTheFallback()
    {
        _primary.GetByZipCodeAsync("99999999", Arg.Any<CancellationToken>()).Returns(AddressErrors.ZipCodeNotFound);

        var result = await _provider.GetByZipCodeAsync("99999999", _ct);

        result.Error.ShouldBe(AddressErrors.ZipCodeNotFound);
        await _fallback.DidNotReceive().GetByZipCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    public static TheoryData<Exception> PrimaryFailures => new()
    {
        new HttpRequestException("fora do ar"),
        new TimeoutRejectedException("demorou demais"),
        new BrokenCircuitException("circuito aberto"),
        new JsonException("resposta quebrada")
    };

    [Theory]
    [MemberData(nameof(PrimaryFailures))]
    public async Task GetByZipCodeAsync_WhenPrimaryFails_UsesTheFallback(Exception failure)
    {
        _primary.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).ThrowsAsync(failure);

        var result = await _provider.GetByZipCodeAsync("01001000", _ct);

        result.Value.Street.ShouldBe("Da reserva");
        await _fallback.Received(1).GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenTheErrorIsABug_DoesNotHideItWithTheFallback()
    {
        _primary.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("bug"));

        await Should.ThrowAsync<InvalidOperationException>(() => _provider.GetByZipCodeAsync("01001000", _ct));
        await _fallback.DidNotReceive().GetByZipCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchByStreetAsync_GoesStraightToThePrimary()
    {
        _primary.SearchByStreetAsync("SP", "São Paulo", "Paulista", Arg.Any<CancellationToken>()).Returns(new List<Address>());

        await _provider.SearchByStreetAsync("SP", "São Paulo", "Paulista", _ct);

        await _primary.Received(1).SearchByStreetAsync("SP", "São Paulo", "Paulista", Arg.Any<CancellationToken>());
    }
}
