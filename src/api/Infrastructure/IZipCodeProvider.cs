using AddressLookup.Api.Common;
using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Infrastructure;

public interface IZipCodeProvider
{
    Task<Result<Address>> GetByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default);
}
