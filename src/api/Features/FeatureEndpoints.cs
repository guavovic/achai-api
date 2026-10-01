using AddressLookup.Api.Features.Addresses;
using AddressLookup.Api.Features.Cities;

namespace AddressLookup.Api.Features;

public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app) =>
        app.MapGetAddressByZipCode()
            .MapSearchAddressesByStreet()
            .MapGetCitiesByState();
}
