namespace AddressLookup.Api.Common.Http;

public static class CorsExtensions
{
    public const string FrontPolicy = "front";

    /// <summary>
    /// Em produção, só as origens de <c>Cors:AllowedOrigins</c> chamam a API pelo navegador.
    /// Em desenvolvimento, qualquer origem, inclusive o index.html aberto direto do disco (origem "null").
    /// </summary>
    public static IServiceCollection AddFrontCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        return services.AddCors(options => options.AddPolicy(FrontPolicy, policy =>
        {
            if (environment.IsDevelopment())
                policy.AllowAnyOrigin();
            else
                policy.WithOrigins(allowedOrigins);

            // A API só tem consultas.
            policy.WithMethods(HttpMethods.Get).AllowAnyHeader();
        }));
    }
}
