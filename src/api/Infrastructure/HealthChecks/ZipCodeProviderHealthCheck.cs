using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Achai.Api.Infrastructure.HealthChecks;

/// <summary>
/// Confere se uma fonte de CEP responde, buscando um CEP que sempre existe.
/// </summary>
public sealed class ZipCodeProviderHealthCheck<TProvider> : IHealthCheck
    where TProvider : IZipCodeProvider
{
    // Praça da Sé, em São Paulo: o exemplo da própria documentação do ViaCEP.
    private const string KnownZipCode = "01001000";

    private readonly TProvider _provider;

    public ZipCodeProviderHealthCheck(TProvider provider)
    {
        _provider = provider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = await _provider.GetByZipCodeAsync(KnownZipCode, cancellationToken);

        return result.IsSuccess
            ? HealthCheckResult.Healthy()
            : new HealthCheckResult(context.Registration.FailureStatus, $"Resposta inesperada: {result.Error!.Code}");
    }
}
