using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Achai.Api.Infrastructure.HealthChecks;

public sealed record StatusSample(DateTimeOffset At, HealthStatus Status, int DurationMs);

public sealed class SourceStatusHistory
{
    public static readonly TimeSpan Window = TimeSpan.FromHours(24);

    private readonly Dictionary<string, List<StatusSample>> _samples = [];
    private readonly Lock _lock = new();
    private readonly TimeProvider _time;

    public SourceStatusHistory(TimeProvider time)
    {
        _time = time;
        Since = time.GetUtcNow();
    }

    public DateTimeOffset Since { get; }

    public void Add(string source, StatusSample sample)
    {
        lock (_lock)
        {
            if (!_samples.TryGetValue(source, out var samples))
                _samples[source] = samples = [];

            samples.Add(sample);
            samples.RemoveAll(old => old.At < _time.GetUtcNow() - Window);
        }
    }

    public IReadOnlyDictionary<string, IReadOnlyList<StatusSample>> Snapshot()
    {
        lock (_lock)
            return _samples.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<StatusSample>)pair.Value.ToList());
    }
}

public sealed class SourceStatusMonitor : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly HealthCheckService _healthChecks;
    private readonly SourceStatusHistory _history;
    private readonly TimeProvider _time;

    public SourceStatusMonitor(HealthCheckService healthChecks, SourceStatusHistory history, TimeProvider time)
    {
        _healthChecks = healthChecks;
        _history = history;
        _time = time;
    }

    public async Task SampleAsync(CancellationToken cancellationToken)
    {
        var report = await _healthChecks.CheckHealthAsync(check => check.Tags.Contains(HealthCheckTags.Ready), cancellationToken);
        var now = _time.GetUtcNow();

        foreach (var (name, entry) in report.Entries)
            _history.Add(name, new StatusSample(now, entry.Status, (int)entry.Duration.TotalMilliseconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, _time);
        do
        {
            try
            {
                await SampleAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
