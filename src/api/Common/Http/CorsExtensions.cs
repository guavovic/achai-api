using System.Text.RegularExpressions;

namespace Achai.Api.Common.Http;

public static class CorsExtensions
{
    public const string FrontPolicy = "front";

    /// <summary>
    /// Em produção, só as origens de <c>Cors:AllowedOrigins</c> e as que casam com algum padrão de
    /// <c>Cors:AllowedOriginPatterns</c> (os links de preview da Vercel, que mudam a cada deploy) chamam a API pelo navegador.
    /// Em desenvolvimento, qualquer origem, inclusive o index.html aberto direto do disco (origem "null").
    /// </summary>
    public static IServiceCollection AddFrontCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var allowedPatterns = (configuration.GetSection("Cors:AllowedOriginPatterns").Get<string[]>() ?? [])
            .Select(pattern => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
            .ToArray();

        return services.AddCors(options => options.AddPolicy(FrontPolicy, policy =>
        {
            if (environment.IsDevelopment())
                policy.AllowAnyOrigin();
            else
                policy.SetIsOriginAllowed(origin =>
                    allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)
                    || allowedPatterns.Any(pattern => pattern.IsMatch(origin)));

            // A API só tem consultas.
            policy.WithMethods(HttpMethods.Get).AllowAnyHeader();
        }));
    }
}
