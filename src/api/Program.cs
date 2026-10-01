using Achai.Api.Common.Http;
using Achai.Api.Features;
using Achai.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Hospedagens como o Render dizem em qual porta escutar pela variável PORT.
// Sem ela, vale o padrão (8080 no container, 5010 no launchSettings).
if (builder.Configuration["PORT"] is { Length: > 0 } port)
    builder.WebHost.UseUrls($"http://+:{port}");

builder.Services.AddFrontCors(builder.Configuration, builder.Environment);
builder.Services.AddPerIpRateLimiting();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidation();
builder.Services.AddPortugueseProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// O CORS vem antes do rate limit para o front conseguir ler a resposta 429.
app.UseCors(CorsExtensions.FrontPolicy);
app.UseRateLimiter();
app.MapFeatureEndpoints();

app.Run();
