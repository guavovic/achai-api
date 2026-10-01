using AddressLookup.Api.Infrastructure.BrasilApi;
using AddressLookup.Api.Infrastructure.Ibge;
using AddressLookup.Api.Infrastructure.ViaCep;
using AddressLookup.Api.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AddressLookup.Api.Tests.Integration;

/// <summary>
/// Sobe a API inteira em memória, com o ViaCEP, o IBGE e a BrasilAPI trocados por handlers falsos.
/// O pipeline de resiliência (timeout, retry e circuit breaker) continua valendo.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FakeHttpMessageHandler ViaCep { get; } = new();
    public FakeHttpMessageHandler Ibge { get; } = new();
    public FakeHttpMessageHandler BrasilApi { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient<ViaCepClient>().ConfigurePrimaryHttpMessageHandler(() => ViaCep);
            services.AddHttpClient<IbgeClient>().ConfigurePrimaryHttpMessageHandler(() => Ibge);
            services.AddHttpClient<BrasilApiClient>().ConfigurePrimaryHttpMessageHandler(() => BrasilApi);
        });
    }
}
