using Achai.Api.Common.Http;
using Achai.Api.Features;
using Achai.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["PORT"] is { Length: > 0 } port)
    builder.WebHost.UseUrls($"http://+:{port}");

builder.Services.AddFrontCors(builder.Configuration, builder.Environment);
builder.Services.AddPerIpRateLimiting();
builder.Services.AddApiDocumentation();
builder.Services.AddValidation();
builder.Services.AddPortugueseProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.FrontPolicy);
app.UseRateLimiter();
app.MapFeatureEndpoints();
app.MapApiDocumentation();

app.Run();
