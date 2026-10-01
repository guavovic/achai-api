using System.Net;
using AddressLookup.Api.Common;
using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Infrastructure.BrasilApi;

public sealed class BrasilApiClient : IZipCodeProvider
{
    public static readonly Uri BaseAddress = new("https://brasilapi.com.br/api/");

    private readonly HttpClient _httpClient;

    public BrasilApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<Address>> GetByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
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

        return address.ToAddress();
    }
}
