using System.Text.Json;
using Achai.Api.Common;
using Achai.Api.Common.Results;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Achai.Api.Infrastructure.Fallback;

/// <summary>
/// Decorator: se a busca por CEP na fonte principal falhar (fora do ar, lenta ou com o circuito aberto),
/// tenta a fonte de reserva. Um "não encontrado" da principal é resposta válida e não aciona a reserva,
/// porque as fontes discordam sobre CEPs que não existem.
/// </summary>
public sealed class FallbackAddressProvider : IAddressProvider
{
    private readonly IAddressProvider _primary;
    private readonly IZipCodeProvider _fallback;
    private readonly ILogger<FallbackAddressProvider> _logger;

    public FallbackAddressProvider(IAddressProvider primary, IZipCodeProvider fallback, ILogger<FallbackAddressProvider> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<Result<Address>> GetByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _primary.GetByZipCodeAsync(zipCode, cancellationToken);
        }
        catch (Exception ex) when (IsExternalFailure(ex))
        {
            _logger.LogWarning(ex, "A fonte principal falhou ao buscar o CEP {ZipCode}. Usando a fonte de reserva.", zipCode);
            return await _fallback.GetByZipCodeAsync(zipCode, cancellationToken);
        }
    }

    public Task<Result<List<Address>>> SearchByStreetAsync(string state, string city, string street, CancellationToken cancellationToken = default) =>
        _primary.SearchByStreetAsync(state, city, street, cancellationToken);

    private static bool IsExternalFailure(Exception ex) =>
        ex is HttpRequestException or TimeoutRejectedException or BrokenCircuitException or JsonException;
}
