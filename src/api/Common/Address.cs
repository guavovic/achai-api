namespace AddressLookup.Api.Common;

/// <summary>
/// Endereço no formato da aplicação, independente da fonte (ViaCEP ou BrasilAPI).
/// </summary>
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
