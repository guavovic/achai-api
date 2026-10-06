using Achai.Api.Infrastructure.BrasilApi;
using Achai.Api.Infrastructure.Ibge;
using Achai.Api.Infrastructure.ViaCep;
using Achai.Api.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Achai.Api.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _environment;
    private readonly IReadOnlyDictionary<string, string> _settings;

    public ApiFactory(string environment = "Development", IReadOnlyDictionary<string, string>? settings = null)
    {
        _environment = environment;
        _settings = settings ?? new Dictionary<string, string>();
    }

    public FakeHttpMessageHandler ViaCep { get; } = new();
    public FakeHttpMessageHandler Ibge { get; } = new();
    public FakeHttpMessageHandler BrasilApi { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.UseSetting("Status:Monitor", "false");

        foreach (var (key, value) in _settings)
            builder.UseSetting(key, value);

        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient<ViaCepClient>().ConfigurePrimaryHttpMessageHandler(() => ViaCep);
            services.AddHttpClient<IbgeClient>().ConfigurePrimaryHttpMessageHandler(() => Ibge);
            services.AddHttpClient<BrasilApiClient>().ConfigurePrimaryHttpMessageHandler(() => BrasilApi);
        });
    }
}
