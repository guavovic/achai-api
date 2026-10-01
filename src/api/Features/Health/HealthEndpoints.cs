using Achai.Api.Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Achai.Api.Features.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        // Liveness: só diz se o processo está de pé, sem olhar nada externo. É o que a hospedagem consulta.
        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false })
            .DisableRateLimiting();

        // Readiness: testa o ViaCEP, a BrasilAPI e o IBGE. Serve para acompanhar, não para reiniciar a API.
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(HealthCheckTags.Ready),
            ResponseWriter = WriteJsonAsync
        });

        return app;
    }

    // Só nome, status e duração de cada fonte: a mensagem de erro fica no log, não na resposta.
    private static Task WriteJsonAsync(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = (int)entry.Value.Duration.TotalMilliseconds
            })
        });
}
