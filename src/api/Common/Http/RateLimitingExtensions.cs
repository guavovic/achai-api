using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace Achai.Api.Common.Http;

public static class RateLimitingExtensions
{
    public const int PermitsPerMinute = 60;

    /// <summary>
    /// Limita cada IP a <see cref="PermitsPerMinute"/> requisições por minuto, em janela fixa.
    /// Quem passa do limite recebe 429 com ProblemDetails e o cabeçalho Retry-After.
    /// </summary>
    public static IServiceCollection AddPerIpRateLimiting(this IServiceCollection services)
    {
        // Atrás do proxy da hospedagem, o IP da conexão é o do proxy. Sem isso, todo mundo cairia no mesmo limite.
        // O proxy pode mudar de IP, então nenhum é fixado. O ForwardLimit padrão (1) usa só a entrada que o
        // próprio proxy acrescenta, que o cliente não consegue forjar.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = PermitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }

                var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails =
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Muitas requisições",
                        Detail = "Você fez muitas requisições em pouco tempo. Tente de novo em até um minuto."
                    }
                });
            };
        });
    }
}
