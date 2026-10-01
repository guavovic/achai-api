using Achai.Api.Common;
using Achai.Api.Infrastructure;
using Achai.Api.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace Achai.Api.Tests.Unit.Infrastructure;

public class CacheExpirationTests : IDisposable
{
    private static readonly Address PracaDaSe = new("01001-000", "Praça da Sé", null, null, null, null, "SP", null, null);

    private readonly ManualClock _clock = new();
    private readonly IAddressProvider _addresses = Substitute.For<IAddressProvider>();
    private readonly ServiceProvider _services;
    private readonly CachedAddressProvider _cached;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public CacheExpirationTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(_clock);
        services.AddMemoryCache(options => options.Clock = _clock);
        services.AddHybridCache();
        _services = services.BuildServiceProvider();
        _cached = new CachedAddressProvider(_addresses, _services.GetRequiredService<HybridCache>());
    }

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task FoundZipCode_StaysCachedForDays()
    {
        _addresses.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).Returns(PracaDaSe);

        await _cached.GetByZipCodeAsync("01001000", _ct);
        _clock.Advance(TimeSpan.FromDays(6));
        await _cached.GetByZipCodeAsync("01001000", _ct);

        await _addresses.Received(1).GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FoundZipCode_ExpiresAfterAWeek()
    {
        _addresses.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).Returns(PracaDaSe);

        await _cached.GetByZipCodeAsync("01001000", _ct);
        _clock.Advance(TimeSpan.FromDays(8));
        await _cached.GetByZipCodeAsync("01001000", _ct);

        await _addresses.Received(2).GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ZipCodeNotFound_ExpiresAfterADay()
    {
        _addresses.GetByZipCodeAsync("99999999", Arg.Any<CancellationToken>()).Returns(AddressErrors.ZipCodeNotFound);

        await _cached.GetByZipCodeAsync("99999999", _ct);
        _clock.Advance(TimeSpan.FromHours(12));
        await _cached.GetByZipCodeAsync("99999999", _ct);
        _clock.Advance(TimeSpan.FromHours(13));
        await _cached.GetByZipCodeAsync("99999999", _ct);

        await _addresses.Received(2).GetByZipCodeAsync("99999999", Arg.Any<CancellationToken>());
    }

    private sealed class ManualClock : TimeProvider, ISystemClock
    {
        private DateTimeOffset _now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public DateTimeOffset UtcNow => _now;

        public void Advance(TimeSpan time) => _now += time;
    }
}
