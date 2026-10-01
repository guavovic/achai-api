using BuscarEnderecos.API.Handlers;
using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Rest;
using BuscarEnderecos.API.Services;

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
builder.Services.AddHttpClient();
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

builder.Services.AddSingleton<IEnderecoService, EnderecoService>();
builder.Services.AddSingleton<IApi, BuscarEnderecosApiRest>();

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
