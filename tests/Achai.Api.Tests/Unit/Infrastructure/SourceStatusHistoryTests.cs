using Achai.Api.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Achai.Api.Tests.Unit.Infrastructure;

public class SourceStatusHistoryTests
{
    private readonly ManualClock _clock = new();

    [Fact]
    public void Add_KeepsOnlyTheLast24Hours()
    {
        var history = new SourceStatusHistory(_clock);

        history.Add("viacep", new StatusSample(_clock.GetUtcNow(), HealthStatus.Healthy, 120));
        _clock.Advance(TimeSpan.FromHours(23));
        history.Add("viacep", new StatusSample(_clock.GetUtcNow(), HealthStatus.Degraded, 2000));
        _clock.Advance(TimeSpan.FromHours(2));
        history.Add("viacep", new StatusSample(_clock.GetUtcNow(), HealthStatus.Healthy, 90));

        history.Snapshot()["viacep"].Select(sample => sample.DurationMs).ShouldBe([2000, 90]);
    }

    [Fact]
    public void Since_IsWhenTheHistoryStarted()
    {
        var started = _clock.GetUtcNow();

        new SourceStatusHistory(_clock).Since.ShouldBe(started);
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan time) => _now += time;
    }
}
