using Achai.Api.Common;
using Achai.Api.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace Achai.Api.Infrastructure.Caching;

public sealed class CachedCoordinatesProvider : ICoordinatesProvider
{
    private static readonly HybridCacheEntryOptions Expiration = HybridCacheExtensions.Expiration(TimeSpan.FromDays(7));
    private static readonly HybridCacheEntryOptions NotFoundExpiration = HybridCacheExtensions.Expiration(TimeSpan.FromDays(1));

    private readonly ICoordinatesProvider _inner;
    private readonly HybridCache _cache;

    public CachedCoordinatesProvider(ICoordinatesProvider inner, HybridCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Task<Result<Coordinates>> GetCoordinatesAsync(string zipCode, CancellationToken cancellationToken = default) =>
        _cache.GetOrCreateResultAsync(
            $"coordenadas:{zipCode}",
            token => _inner.GetCoordinatesAsync(zipCode, token),
            Expiration,
            cancellationToken,
            NotFoundExpiration);
}
