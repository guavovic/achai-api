using BuscarEnderecos.API.Caching;
using BuscarEnderecos.API.Handlers;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Rest;
using BuscarEnderecos.API.Services;
using BuscarEnderecos.API.Settings;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("front",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(BuscarEnderecosApiRest.ViaCepClient, client => client.BaseAddress = new Uri(ApiUrls.VIA_CEP))
    .AddStandardResilienceHandler(ConfigurarResiliencia);
builder.Services.AddHttpClient(BuscarEnderecosApiRest.IbgeClient, client => client.BaseAddress = new Uri(ApiUrls.IBGE))
    .AddStandardResilienceHandler(ConfigurarResiliencia);
builder.Services.AddHttpClient(BrasilApiRest.BrasilApiClient, client => client.BaseAddress = new Uri(ApiUrls.BRASIL_API))
    .AddStandardResilienceHandler(ConfigurarResiliencia);
builder.Services.AddProblemDetails(options =>
{
    // Troca os títulos padrão (em inglês) de todo ProblemDetails, venha do MVC ou do pipeline.
    options.CustomizeProblemDetails = context =>
    {
        var problem = context.ProblemDetails;

        problem.Title = problem switch
        {
            HttpValidationProblemDetails => "Um ou mais campos são inválidos.",
            { Status: StatusCodes.Status400BadRequest } => "Requisição inválida",
            { Status: StatusCodes.Status404NotFound } => "Não encontrado",
            _ => problem.Title
        };
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHybridCache();

builder.Services.AddSingleton<IEnderecoService, EnderecoService>();
builder.Services.AddSingleton<BuscarEnderecosApiRest>();
builder.Services.AddSingleton<ICepReserva, BrasilApiRest>();
// O IApi entregue ao service é uma cadeia de decorators: cache → fallback para a BrasilAPI → ViaCEP/IBGE.
builder.Services.AddSingleton<IApi>(sp => new CachedApi(
    new FallbackApi(
        sp.GetRequiredService<BuscarEnderecosApiRest>(),
        sp.GetRequiredService<ICepReserva>(),
        sp.GetRequiredService<ILogger<FallbackApi>>()),
    sp.GetRequiredService<HybridCache>()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("front");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Busca de endereço precisa responder rápido: o padrão do pacote espera até 30s no total.
// O circuit breaker padrão só abre depois de 100 requisições em 30s, o que nunca acontece no volume desta API.
static void ConfigurarResiliencia(HttpStandardResilienceOptions options)
{
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(6);

    options.Retry.MaxRetryAttempts = 2;
    options.Retry.Delay = TimeSpan.FromMilliseconds(200);

    options.CircuitBreaker.MinimumThroughput = 5;
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
}
