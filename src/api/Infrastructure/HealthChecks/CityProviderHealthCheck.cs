using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Achai.Api.Infrastructure.HealthChecks;

/// <summary>
/// Confere se a fonte de cidades responde, buscando as cidades de uma UF.
/// </summary>
public sealed class CityProviderHealthCheck<TProvider> : IHealthCheck
    where TProvider : ICityProvider
{
    // O Acre tem só 22 municípios, a menor resposta entre as UFs.
    private const string SmallState = "AC";

    private readonly TProvider _provider;

    public CityProviderHealthCheck(TProvider provider)
    {
        _provider = provider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = await _provider.GetByStateAsync(SmallState, cancellationToken);

        return result.IsSuccess
            ? HealthCheckResult.Healthy()
            : new HealthCheckResult(context.Registration.FailureStatus, $"Resposta inesperada: {result.Error!.Code}");
    }
}
