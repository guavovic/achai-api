using AddressLookup.Api.Common;
using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Infrastructure.Ibge;

public sealed class IbgeClient : ICityProvider
{
    public static readonly Uri BaseAddress = new("https://servicodados.ibge.gov.br/api/v1/");

    private readonly HttpClient _httpClient;

    public IbgeClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<List<City>>> GetByStateAsync(string state, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"localidades/estados/{state}/municipios", cancellationToken);

        response.EnsureSuccessStatusCode();

        var cities = await response.Content.ReadFromJsonAsync<List<IbgeCity>>(cancellationToken);

        // O IBGE devolve 200 com lista vazia para uma UF que não existe.
        if (cities is null || cities.Count == 0)
            return AddressErrors.StateNotFound;

        return cities.Select(city => new City(city.Nome)).ToList();
    }
}
