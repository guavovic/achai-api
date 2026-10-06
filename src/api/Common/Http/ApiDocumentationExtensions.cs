using Microsoft.OpenApi;

namespace Achai.Api.Common.Http;

public static class ApiDocumentationExtensions
{
    public const string DocsPath = "/docs";

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "achaí-API",
                Version = "v1",
                Description = "Busca endereços brasileiros pelo CEP ou pelo logradouro, e lista as cidades de cada estado. " +
                    "Os dados vêm do ViaCEP, com a BrasilAPI como alternativa para CEP, e do IBGE. " +
                    "Erros seguem o ProblemDetails (RFC 9457), e cada IP pode fazer 60 requisições por minuto."
            };

            return Task.CompletedTask;
        }));

    public static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();

        var docsUrl = app.Configuration["Docs:Url"]
            ?? throw new InvalidOperationException("Configure Docs:Url com o endereço da documentação no front.");
        app.MapGet(DocsPath, () => TypedResults.Redirect(docsUrl)).ExcludeFromDescription();

        return app;
    }
}
