using AddressLookup.Api.Common;
using AddressLookup.Api.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace AddressLookup.Api.Infrastructure.Caching;

/// <summary>
/// Decorator: consulta o cache antes da fonte de endereços.
/// </summary>
public sealed class CachedAddressProvider : IAddressProvider
{
    private static readonly HybridCacheEntryOptions ZipCodeExpiration = HybridCacheExtensions.Expiration(TimeSpan.FromHours(24));
    private static readonly HybridCacheEntryOptions StreetExpiration = HybridCacheExtensions.Expiration(TimeSpan.FromHours(1));

    private readonly IAddressProvider _inner;
    private readonly HybridCache _cache;

    public CachedAddressProvider(IAddressProvider inner, HybridCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Task<Result<Address>> GetByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default) =>
        _cache.GetOrCreateResultAsync(
            $"cep:{zipCode}",
            token => _inner.GetByZipCodeAsync(zipCode, token),
            ZipCodeExpiration,
            cancellationToken);

    public Task<Result<List<Address>>> SearchByStreetAsync(string state, string city, string street, CancellationToken cancellationToken = default) =>
        _cache.GetOrCreateResultAsync(
            $"logradouro:{state}:{city}:{street}",
            token => _inner.SearchByStreetAsync(state, city, street, token),
            StreetExpiration,
            cancellationToken);
}
