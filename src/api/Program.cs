using BuscarEnderecos.API.Interfaces;
using BuscarEnderecos.API.Mapping;
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

builder.Services.AddSingleton<IEnderecoService, EnderecoService>();
builder.Services.AddSingleton<IApi, BuscarEnderecosApiRest>();
builder.Services.AddAutoMapper(typeof(AddressMapping));

var app = builder.Build();

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
