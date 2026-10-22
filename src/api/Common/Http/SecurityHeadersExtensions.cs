namespace Achai.Api.Common.Http;

public static class SecurityHeadersExtensions
{
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services) =>
        services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use((context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers.XFrameOptions = "DENY";
                headers["Referrer-Policy"] = "no-referrer";
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
                return Task.CompletedTask;
            });

            return next(context);
        });

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        return app;
    }
}
