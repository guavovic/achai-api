using Achai.Api.Common;
using Achai.Api.Common.Results;

namespace Achai.Api.Infrastructure;

public interface ICityProvider
{
    Task<Result<List<City>>> GetByStateAsync(string state, CancellationToken cancellationToken = default);
}
