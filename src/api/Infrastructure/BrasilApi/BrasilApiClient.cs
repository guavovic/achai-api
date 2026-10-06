using System.Net;
using Achai.Api.Common;
using Achai.Api.Common.Results;

namespace Achai.Api.Infrastructure.BrasilApi;

public sealed class BrasilApiClient : IZipCodeProvider, ICoordinatesProvider
{
    public static readonly Uri BaseAddress = new("https://brasilapi.com.br/api/");

    private readonly HttpClient _httpClient;

    public BrasilApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<Address>> GetByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        var result = await GetAsync(zipCode, cancellationToken);
        return result.Map(address => address.ToAddress());
    }

    public async Task<Result<Coordinates>> GetCoordinatesAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        var result = await GetAsync(zipCode, cancellationToken);
        if (!result.IsSuccess)
            return result.Error!;

        return result.Value.ToCoordinates() is { } coordinates ? coordinates : CoordinatesErrors.NotFound;
    }

    private async Task<Result<BrasilApiAddress>> GetAsync(string zipCode, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"cep/v2/{zipCode}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return AddressErrors.InvalidZipCode;

        if (response.StatusCode == HttpStatusCode.NotFound)
            return AddressErrors.ZipCodeNotFound;

        response.EnsureSuccessStatusCode();

        var address = await response.Content.ReadFromJsonAsync<BrasilApiAddress>(cancellationToken);

        if (address is null)
            return AddressErrors.ZipCodeNotFound;

        return address;
    }
}
