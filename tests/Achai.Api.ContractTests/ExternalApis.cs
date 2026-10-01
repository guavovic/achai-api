using Achai.Api.Infrastructure;
using Achai.Api.Infrastructure.BrasilApi;
using Achai.Api.Infrastructure.Ibge;
using Achai.Api.Infrastructure.ViaCep;
using Microsoft.Extensions.DependencyInjection;

namespace Achai.Api.ContractTests;

public sealed class ExternalApis : IDisposable
{
    private readonly ServiceProvider _services = new ServiceCollection()
        .AddLogging()
        .AddInfrastructure()
        .BuildServiceProvider();

    public ViaCepClient ViaCep => _services.GetRequiredService<ViaCepClient>();
    public BrasilApiClient BrasilApi => _services.GetRequiredService<BrasilApiClient>();
    public IbgeClient Ibge => _services.GetRequiredService<IbgeClient>();

    public void Dispose() => _services.Dispose();
}
