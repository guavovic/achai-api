using BuscarEnderecos.API.Rest;
using BuscarEnderecos.API.Tests.Fakes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BuscarEnderecos.API.Tests.Integration
{
    /// <summary>
    /// Sobe a API inteira em memória, com o ViaCEP e o IBGE trocados por handlers falsos.
    /// </summary>
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        public FakeHttpMessageHandler ViaCep { get; } = new();
        public FakeHttpMessageHandler Ibge { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddHttpClient(BuscarEnderecosApiRest.ViaCepClient)
                    .ConfigurePrimaryHttpMessageHandler(() => ViaCep);
                services.AddHttpClient(BuscarEnderecosApiRest.IbgeClient)
                    .ConfigurePrimaryHttpMessageHandler(() => Ibge);
            });
        }
    }
}
