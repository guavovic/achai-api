using AddressLookup.Api.Common;
using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Infrastructure;

public interface IAddressProvider : IZipCodeProvider
{
    Task<Result<List<Address>>> SearchByStreetAsync(string state, string city, string street, CancellationToken cancellationToken = default);
}
