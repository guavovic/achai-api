using AddressLookup.Api.Common;
using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Infrastructure;

public interface ICityProvider
{
    Task<Result<List<City>>> GetByStateAsync(string state, CancellationToken cancellationToken = default);
}
