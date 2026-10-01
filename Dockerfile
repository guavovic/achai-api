# Estágio de build: compila e publica com o SDK completo.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura antes de copiar o código: quando só o código muda, o Docker reaproveita esta camada.
COPY src/api/Achai.Api.csproj src/api/
RUN dotnet restore src/api/Achai.Api.csproj

COPY src/api/ src/api/
RUN dotnet publish src/api/Achai.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app \
    -p:UseAppHost=false

# Estágio final: runtime "chiseled", só com o que o .NET precisa, sem shell e sem root.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .

# Porta padrão das imagens .NET. Hospedagens que mandam a variável PORT sobrescrevem (ver Program.cs).
EXPOSE 8080

ENTRYPOINT ["dotnet", "Achai.Api.dll"]
