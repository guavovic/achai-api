using Achai.Api.Common;
using Achai.Api.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace Achai.Api.Infrastructure.Caching;

/// <summary>
/// Decorator: consulta o cache antes da fonte de cidades.
/// </summary>
public sealed class CachedCityProvider : ICityProvider
{
    private static readonly HybridCacheEntryOptions CitiesExpiration = HybridCacheExtensions.Expiration(TimeSpan.FromDays(7));

    private readonly ICityProvider _inner;
    private readonly HybridCache _cache;

    public CachedCityProvider(ICityProvider inner, HybridCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Task<Result<List<City>>> GetByStateAsync(string state, CancellationToken cancellationToken = default) =>
        _cache.GetOrCreateResultAsync(
            $"cidades:{state}",
            token => _inner.GetByStateAsync(state, token),
            CitiesExpiration,
            cancellationToken);
}
