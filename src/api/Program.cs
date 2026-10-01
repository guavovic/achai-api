using AddressLookup.Api.Common.Http;
using AddressLookup.Api.Features;
using AddressLookup.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
