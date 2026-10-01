FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/api/Achai.Api.csproj src/api/
RUN dotnet restore src/api/Achai.Api.csproj

COPY src/api/ src/api/
RUN dotnet publish src/api/Achai.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Achai.Api.dll"]
