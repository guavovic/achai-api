using AddressLookup.Api.Common;
using AddressLookup.Api.Infrastructure;
using AddressLookup.Api.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace AddressLookup.Api.Tests.Unit.Infrastructure;

public class CachedProvidersTests : IDisposable
{
    private static readonly Address PracaDaSe = new("01001-000", "Praça da Sé", null, null, null, null, "SP", null, null);
    private static readonly Address Paulista = new("01310-100", "Avenida Paulista", null, null, null, null, "SP", null, null);

    private readonly IAddressProvider _addresses = Substitute.For<IAddressProvider>();
    private readonly ICityProvider _cities = Substitute.For<ICityProvider>();
    private readonly ServiceProvider _services;
    private readonly CachedAddressProvider _cachedAddresses;
    private readonly CachedCityProvider _cachedCities;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public CachedProvidersTests()
    {
        _services = new ServiceCollection().AddHybridCache().Services.BuildServiceProvider();
        var cache = _services.GetRequiredService<HybridCache>();
        _cachedAddresses = new CachedAddressProvider(_addresses, cache);
        _cachedCities = new CachedCityProvider(_cities, cache);
    }

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task GetByZipCodeAsync_SecondTime_UsesTheCache()
    {
        _addresses.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).Returns(PracaDaSe);

        await _cachedAddresses.GetByZipCodeAsync("01001000", _ct);
        var result = await _cachedAddresses.GetByZipCodeAsync("01001000", _ct);

        result.Value.Street.ShouldBe("Praça da Sé");
        await _addresses.Received(1).GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByZipCodeAsync_DifferentZipCodes_DoNotShareEntries()
    {
        _addresses.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).Returns(PracaDaSe);
        _addresses.GetByZipCodeAsync("01310100", Arg.Any<CancellationToken>()).Returns(Paulista);

        var se = await _cachedAddresses.GetByZipCodeAsync("01001000", _ct);
        var paulista = await _cachedAddresses.GetByZipCodeAsync("01310100", _ct);

        se.Value.Street.ShouldBe("Praça da Sé");
        paulista.Value.Street.ShouldBe("Avenida Paulista");
    }

    [Fact]
    public async Task GetByZipCodeAsync_ZipCodeNotFound_IsAlsoCached()
    {
        _addresses.GetByZipCodeAsync("99999999", Arg.Any<CancellationToken>()).Returns(AddressErrors.ZipCodeNotFound);

        await _cachedAddresses.GetByZipCodeAsync("99999999", _ct);
        var result = await _cachedAddresses.GetByZipCodeAsync("99999999", _ct);

        result.Error.ShouldBe(AddressErrors.ZipCodeNotFound);
        await _addresses.Received(1).GetByZipCodeAsync("99999999", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByZipCodeAsync_WhenTheSourceFails_DoesNotCacheAndTriesAgain()
    {
        _addresses.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>())
            .Returns(_ => throw new HttpRequestException("ViaCEP fora do ar"), _ => PracaDaSe);

        await Should.ThrowAsync<HttpRequestException>(() => _cachedAddresses.GetByZipCodeAsync("01001000", _ct));
        var result = await _cachedAddresses.GetByZipCodeAsync("01001000", _ct);

        result.Value.Street.ShouldBe("Praça da Sé");
        await _addresses.Received(2).GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchByStreetAsync_SecondTime_UsesTheCache()
    {
        _addresses.SearchByStreetAsync("SP", "São Paulo", "Paulista", Arg.Any<CancellationToken>())
            .Returns(new List<Address> { Paulista });

        await _cachedAddresses.SearchByStreetAsync("SP", "São Paulo", "Paulista", _ct);
        var result = await _cachedAddresses.SearchByStreetAsync("SP", "São Paulo", "Paulista", _ct);

        result.Value.Single().ZipCode.ShouldBe("01310-100");
        await _addresses.Received(1).SearchByStreetAsync("SP", "São Paulo", "Paulista", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByStateAsync_SecondTime_UsesTheCache()
    {
        _cities.GetByStateAsync("AC", Arg.Any<CancellationToken>()).Returns(new List<City> { new("Acrelândia") });

        await _cachedCities.GetByStateAsync("AC", _ct);
        var result = await _cachedCities.GetByStateAsync("AC", _ct);

        result.Value.Single().Name.ShouldBe("Acrelândia");
        await _cities.Received(1).GetByStateAsync("AC", Arg.Any<CancellationToken>());
    }
}
