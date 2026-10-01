using BuscarEnderecos.API.Caching;
using BuscarEnderecos.API.Handlers;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Rest;
using BuscarEnderecos.API.Services;
using BuscarEnderecos.API.Settings;
using Microsoft.Extensions.Caching.Hybrid;

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
builder.Services.AddHttpClient(BuscarEnderecosApiRest.ViaCepClient, client => client.BaseAddress = new Uri(ApiUrls.VIA_CEP));
builder.Services.AddHttpClient(BuscarEnderecosApiRest.IbgeClient, client => client.BaseAddress = new Uri(ApiUrls.IBGE));
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
// O IApi entregue ao service é o decorator com cache, que por dentro chama o cliente real.
builder.Services.AddSingleton<IApi>(sp => new CachedApi(
    sp.GetRequiredService<BuscarEnderecosApiRest>(),
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
