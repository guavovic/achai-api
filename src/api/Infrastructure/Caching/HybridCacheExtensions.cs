using Achai.Api.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace Achai.Api.Infrastructure.Caching;

public static class HybridCacheExtensions
{
    /// <summary>
    /// Guarda o resultado inteiro (valor ou erro esperado). Exceções não ficam no cache,
    /// então uma falha da fonte externa é tentada de novo na próxima requisição.
    /// </summary>
    public static async Task<Result<T>> GetOrCreateResultAsync<T>(
        this HybridCache cache,
        string key,
        Func<CancellationToken, Task<Result<T>>> factory,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken)
    {
        var cached = await cache.GetOrCreateAsync(
            key,
            async token => CachedResult<T>.From(await factory(token)),
            options,
            cancellationToken: cancellationToken);

        return cached.ToResult();
    }

    public static HybridCacheEntryOptions Expiration(TimeSpan duration) => new()
    {
        Expiration = duration,
        LocalCacheExpiration = duration
    };
}
