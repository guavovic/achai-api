using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Achai.Api.Infrastructure.HealthChecks;

public sealed class ZipCodeProviderHealthCheck<TProvider> : IHealthCheck
    where TProvider : IZipCodeProvider
{
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
