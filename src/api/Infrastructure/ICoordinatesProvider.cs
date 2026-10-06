using Achai.Api.Common;
using Achai.Api.Common.Results;

namespace Achai.Api.Infrastructure;

public interface ICoordinatesProvider
{
    Task<Result<Coordinates>> GetCoordinatesAsync(string zipCode, CancellationToken cancellationToken = default);
}
