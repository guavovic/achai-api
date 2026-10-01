using Achai.Api.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace Achai.Api.Infrastructure.Caching;

public static class HybridCacheExtensions
{
    public static async Task<Result<T>> GetOrCreateResultAsync<T>(
        this HybridCache cache,
        string key,
        Func<CancellationToken, Task<Result<T>>> factory,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken,
        HybridCacheEntryOptions? errorOptions = null)
    {
        var fetched = false;

        var cached = await cache.GetOrCreateAsync(
            key,
            async token =>
            {
                fetched = true;
                return CachedResult<T>.From(await factory(token));
            },
            options,
            cancellationToken: cancellationToken);

        if (fetched && cached.Error is not null && errorOptions is not null)
            await cache.SetAsync(key, cached, errorOptions, cancellationToken: cancellationToken);

        return cached.ToResult();
    }

    public static HybridCacheEntryOptions Expiration(TimeSpan duration) => new()
    {
        Expiration = duration,
        LocalCacheExpiration = duration
    };
}
