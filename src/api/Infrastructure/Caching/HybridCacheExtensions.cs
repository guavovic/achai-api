using Achai.Api.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace Achai.Api.Infrastructure.Caching;

public static class HybridCacheExtensions
{
    /// <summary>
    /// Guarda o resultado inteiro (valor ou erro esperado). Exceções não ficam no cache,
    /// então uma falha da fonte externa é tentada de novo na próxima requisição.
    /// Um erro esperado fica pelo tempo de <paramref name="errorOptions"/>, quando informado.
    /// </summary>
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

        // Só regrava quando a fonte acabou de ser consultada; uma leitura do cache não renova a validade.
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
