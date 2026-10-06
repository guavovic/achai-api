using System.Text.Json.Serialization;
using Achai.Api.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Achai.Api.Features.Health;

public sealed record SampleResponse(
    [property: JsonPropertyName("em")] DateTimeOffset At,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("ms")] int DurationMs);

public sealed record SourceStatusResponse(
    [property: JsonPropertyName("fonte")] string Source,
    [property: JsonPropertyName("disponibilidade")] double Availability,
    [property: JsonPropertyName("latenciaMediaMs")] int AverageLatencyMs,
    [property: JsonPropertyName("ultimoStatus")] string LastStatus,
    [property: JsonPropertyName("amostras")] List<SampleResponse> Samples);

public sealed record StatusResponse(
    [property: JsonPropertyName("desde")] DateTimeOffset Since,
    [property: JsonPropertyName("intervaloMinutos")] int IntervalMinutes,
    [property: JsonPropertyName("fontes")] List<SourceStatusResponse> Sources);

public static class GetSourcesStatus
{
    private static readonly Dictionary<string, string> Names = new()
    {
        ["viacep"] = "ViaCEP",
        ["brasilapi"] = "BrasilAPI",
        ["ibge"] = "IBGE",
    };

    public static IEndpointRouteBuilder MapGetSourcesStatus(this IEndpointRouteBuilder app)
    {
        app.MapGet("/status", Handle)
            .WithName(nameof(GetSourcesStatus))
            .WithTags("Status")
            .WithSummary("Disponibilidade das fontes")
            .WithDescription(
                "Histórico das verificações do ViaCEP, da BrasilAPI e do IBGE, feitas a cada 5 minutos e guardadas em memória " +
                "por até 24 horas. O servidor dorme quando fica sem uso, e o histórico recomeça quando ele acorda: o campo " +
                "desde diz a partir de quando há dados. Disponibilidade é a porcentagem de verificações saudáveis.")
            .Produces<StatusResponse>()
            .DisableRateLimiting();

        return app;
    }

    public static IResult Handle(SourceStatusHistory history)
    {
        var sources = history.Snapshot()
            .OrderBy(pair => Array.IndexOf(Names.Keys.ToArray(), pair.Key))
            .Select(pair => ToResponse(pair.Key, pair.Value))
            .ToList();

        return TypedResults.Ok(new StatusResponse(history.Since, (int)SourceStatusMonitor.Interval.TotalMinutes, sources));
    }

    private static SourceStatusResponse ToResponse(string name, IReadOnlyList<StatusSample> samples)
    {
        var healthy = samples.Count(sample => sample.Status == HealthStatus.Healthy);

        return new SourceStatusResponse(
            Names.GetValueOrDefault(name, name),
            samples.Count == 0 ? 0 : Math.Round(100.0 * healthy / samples.Count, 1),
            samples.Count == 0 ? 0 : (int)samples.Average(sample => sample.DurationMs),
            samples.Count == 0 ? "sem dados" : Label(samples[^1].Status),
            samples.Select(sample => new SampleResponse(sample.At, Label(sample.Status), sample.DurationMs)).ToList());
    }

    private static string Label(HealthStatus status) => status == HealthStatus.Healthy ? "ok" : "fora";
}
