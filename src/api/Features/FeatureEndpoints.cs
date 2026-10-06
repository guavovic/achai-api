using Achai.Api.Features.Addresses;
using Achai.Api.Features.Cities;
using Achai.Api.Features.Geo;
using Achai.Api.Features.Health;

namespace Achai.Api.Features;

public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app) =>
        app.MapGetAddressByZipCode()
            .MapGetAddressesByZipCodes()
            .MapCompareZipCodeSources()
            .MapSearchAddressesByStreet()
            .MapGetCitiesByState()
            .MapGeoEndpoints()
            .MapHealthEndpoints();
}
