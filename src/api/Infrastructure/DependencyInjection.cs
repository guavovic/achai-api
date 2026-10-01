using Achai.Api.Infrastructure.BrasilApi;
using Achai.Api.Infrastructure.Caching;
using Achai.Api.Infrastructure.Fallback;
using Achai.Api.Infrastructure.HealthChecks;
using Achai.Api.Infrastructure.Ibge;
using Achai.Api.Infrastructure.ViaCep;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http.Resilience;

namespace Achai.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient<ViaCepClient>(client => client.BaseAddress = ViaCepClient.BaseAddress)
            .AddStandardResilienceHandler(ConfigureResilience);
        services.AddHttpClient<IbgeClient>(client => client.BaseAddress = IbgeClient.BaseAddress)
            .AddStandardResilienceHandler(ConfigureResilience);
        services.AddHttpClient<BrasilApiClient>(client => client.BaseAddress = BrasilApiClient.BaseAddress)
            .AddStandardResilienceHandler(ConfigureResilience);

        services.AddHybridCache();

        // As features recebem cadeias de decorators:
        // endereços: cache → fallback para a BrasilAPI → ViaCEP; cidades: cache → IBGE.
        services.AddScoped<IAddressProvider>(sp => new CachedAddressProvider(
            new FallbackAddressProvider(
                sp.GetRequiredService<ViaCepClient>(),
                sp.GetRequiredService<BrasilApiClient>(),
                sp.GetRequiredService<ILogger<FallbackAddressProvider>>()),
            sp.GetRequiredService<HybridCache>()));

        services.AddScoped<ICityProvider>(sp => new CachedCityProvider(
            sp.GetRequiredService<IbgeClient>(),
            sp.GetRequiredService<HybridCache>()));

        // As fontes externas entram só na readiness. Uma fonte fora deixa a API "Degraded", não "Unhealthy",
        // porque ela continua respondendo (com fallback ou com erro tratado).
        string[] ready = [HealthCheckTags.Ready];
        var timeout = TimeSpan.FromSeconds(3);
        services.AddHealthChecks()
            .AddCheck<ZipCodeProviderHealthCheck<ViaCepClient>>("viacep", HealthStatus.Degraded, ready, timeout)
            .AddCheck<ZipCodeProviderHealthCheck<BrasilApiClient>>("brasilapi", HealthStatus.Degraded, ready, timeout)
            .AddCheck<CityProviderHealthCheck<IbgeClient>>("ibge", HealthStatus.Degraded, ready, timeout);

        return services;
    }

    // Busca de endereço precisa responder rápido: o padrão do pacote espera até 30s no total.
    // O circuit breaker padrão só abre depois de 100 requisições em 30s, o que nunca acontece no volume desta API.
    private static void ConfigureResilience(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(6);

        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);

        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    }
}
