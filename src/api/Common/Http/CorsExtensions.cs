using System.Text.RegularExpressions;

namespace Achai.Api.Common.Http;

public static class CorsExtensions
{
    public const string FrontPolicy = "front";

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

            policy.WithMethods(HttpMethods.Get, HttpMethods.Post).AllowAnyHeader().WithExposedHeaders("X-Logradouro-Buscado");
        }));
    }
}
