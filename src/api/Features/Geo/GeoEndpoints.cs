using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Achai.Api.Common;
using Achai.Api.Common.Http;
using Achai.Api.Common.Results;
using Achai.Api.Infrastructure;

namespace Achai.Api.Features.Geo;

public sealed record CoordinatesResponse(
    [property: JsonPropertyName("cep")] string ZipCode,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("fonte")] string Source);

public sealed record DistanceResponse(
    [property: JsonPropertyName("origem")] CoordinatesResponse Origin,
    [property: JsonPropertyName("destino")] CoordinatesResponse Destination,
    [property: JsonPropertyName("distanciaKm")] double DistanceKm);

public static class GeoEndpoints
{
    private const string ZipCodePattern = @"^\d{5}-?\d{3}$";
    private const string ZipCodeMessage = "O CEP deve ter 8 dígitos, com ou sem traço.";
    private const string Source = "BrasilAPI";

    public static IEndpointRouteBuilder MapGeoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/buscar/{cep}/coordenadas", GetCoordinatesAsync)
            .WithName("GetCoordinatesByZipCode")
            .WithTags("Localização")
            .WithSummary("Coordenadas do CEP")
            .WithDescription(
                "Latitude e longitude do CEP, vindas da BrasilAPI. São aproximadas: na maioria dos CEPs, apontam para o centro " +
                "da cidade, não para a rua. CEP sem coordenadas devolve 404.")
            .Produces<CoordinatesResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        app.MapGet("/distancia/{origem}/{destino}", GetDistanceAsync)
            .WithName("GetDistanceBetweenZipCodes")
            .WithTags("Localização")
            .WithSummary("Distância entre dois CEPs")
            .WithDescription(
                "Distância em linha reta, em quilômetros, entre as coordenadas dos dois CEPs (fórmula de haversine). " +
                "Como as coordenadas costumam ser o centro da cidade, serve para distância entre cidades, não dentro da mesma " +
                "cidade. Não é a distância de carro. Se um dos CEPs não tiver coordenadas, devolve 404 dizendo qual.")
            .Produces<DistanceResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    public static async Task<IResult> GetCoordinatesAsync(
        [Description("CEP com 8 dígitos, com ou sem traço. Exemplo: 01001000.")]
        [RegularExpression(ZipCodePattern, ErrorMessage = ZipCodeMessage)] string cep,
        ICoordinatesProvider coordinatesProvider,
        CancellationToken cancellationToken)
    {
        var zipCode = cep.Replace("-", "");
        var result = await coordinatesProvider.GetCoordinatesAsync(zipCode, cancellationToken);

        return result.Match(
            coordinates => TypedResults.Ok(ToResponse(zipCode, coordinates)),
            error => error.ToProblem());
    }

    public static async Task<IResult> GetDistanceAsync(
        [Description("CEP de origem. Exemplo: 01001000.")]
        [RegularExpression(ZipCodePattern, ErrorMessage = ZipCodeMessage)] string origem,
        [Description("CEP de destino. Exemplo: 20040020.")]
        [RegularExpression(ZipCodePattern, ErrorMessage = ZipCodeMessage)] string destino,
        ICoordinatesProvider coordinatesProvider,
        CancellationToken cancellationToken)
    {
        var origin = origem.Replace("-", "");
        var destination = destino.Replace("-", "");
        var originResult = await coordinatesProvider.GetCoordinatesAsync(origin, cancellationToken);
        var destinationResult = await coordinatesProvider.GetCoordinatesAsync(destination, cancellationToken);

        if (!originResult.IsSuccess)
            return WithZipCode(originResult.Error!, "origem", origin).ToProblem();
        if (!destinationResult.IsSuccess)
            return WithZipCode(destinationResult.Error!, "destino", destination).ToProblem();

        var distance = originResult.Value.DistanceKmTo(destinationResult.Value);

        return TypedResults.Ok(new DistanceResponse(
            ToResponse(origin, originResult.Value),
            ToResponse(destination, destinationResult.Value),
            Math.Round(distance, 2)));
    }

    private static CoordinatesResponse ToResponse(string zipCode, Coordinates coordinates) =>
        new(zipCode, coordinates.Latitude, coordinates.Longitude, Source);

    private static Error WithZipCode(Error error, string role, string zipCode) =>
        error with { Description = $"{error.Description.TrimEnd('.')} ({role}: {zipCode})." };
}
