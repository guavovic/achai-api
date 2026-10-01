using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Achai.Api.Common.Http;

public static class ApiDocumentationExtensions
{
    public const string DocsPath = "/docs";

    /// <summary>
    /// Gera o documento OpenAPI a partir das rotas, com o OpenAPI nativo do ASP.NET Core.
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Achaí",
                Version = "v1",
                Description = "Busca endereços brasileiros pelo CEP ou pelo logradouro, e lista as cidades de cada estado. " +
                    "Os dados vêm do ViaCEP, com a BrasilAPI como alternativa para CEP, e do IBGE. " +
                    "Erros seguem o ProblemDetails (RFC 9457), e cada IP pode fazer 60 requisições por minuto."
            };

            return Task.CompletedTask;
        }));

    /// <summary>
    /// Publica o documento em /openapi/v1.json e a referência interativa (Scalar) em <see cref="DocsPath"/>,
    /// inclusive em produção: a API é pública e só tem consultas, então a documentação também é.
    /// </summary>
    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(DocsPath, options => options
            .WithTitle("Achaí")
            .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch));

        return app;
    }
}
