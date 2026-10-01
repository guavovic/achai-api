using System.Net;
using Achai.Api.Common;
using Achai.Api.Common.Results;

namespace Achai.Api.Infrastructure.ViaCep;

public sealed class ViaCepClient : IAddressProvider
{
    public static readonly Uri BaseAddress = new("https://viacep.com.br/ws/");

    private readonly HttpClient _httpClient;

    public ViaCepClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<Address>> GetByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{zipCode}/json", cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return AddressErrors.InvalidZipCode;

        response.EnsureSuccessStatusCode();

        var address = await response.Content.ReadFromJsonAsync<ViaCepAddress>(cancellationToken);

        if (address is null || address.NotFound)
            return AddressErrors.ZipCodeNotFound;

        return address.ToAddress();
    }

    public async Task<Result<List<Address>>> SearchByStreetAsync(string state, string city, string street, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{state}/{city}/{street}/json", cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
            return AddressErrors.InvalidStreetSearch;

        response.EnsureSuccessStatusCode();

        var addresses = await response.Content.ReadFromJsonAsync<List<ViaCepAddress>>(cancellationToken);

        return addresses?.Select(address => address.ToAddress()).ToList() ?? [];
    }
}
