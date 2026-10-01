using Achai.Api.Common;
using Achai.Api.Common.Results;

namespace Achai.Api.Infrastructure;

public interface IZipCodeProvider
{
    Task<Result<Address>> GetByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default);
}
