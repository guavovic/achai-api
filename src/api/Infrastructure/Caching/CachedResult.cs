using Achai.Api.Common.Results;

namespace Achai.Api.Infrastructure.Caching;

public sealed record CachedResult<T>(T? Value, Error? Error)
{
    public static CachedResult<T> From(Result<T> result) =>
        result.Match(value => new CachedResult<T>(value, null), error => new CachedResult<T>(default, error));

    public Result<T> ToResult() =>
        Error is null ? Result<T>.Success(Value!) : Result<T>.Failure(Error);
}
