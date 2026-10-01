using Achai.Api.Common;
using Achai.Api.Common.Results;

namespace Achai.Api.Infrastructure;

public interface IAddressProvider : IZipCodeProvider
{
    Task<Result<List<Address>>> SearchByStreetAsync(string state, string city, string street, CancellationToken cancellationToken = default);
}
