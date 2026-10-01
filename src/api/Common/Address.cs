namespace Achai.Api.Common;

public sealed record Address(
    string? ZipCode,
    string? Street,
    string? Complement,
    string? Unit,
    string? Neighborhood,
    string? City,
    string? State,
    string? StateName,
    string? Region);
