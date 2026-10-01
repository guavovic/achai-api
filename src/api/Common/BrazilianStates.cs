namespace Achai.Api.Common;

public sealed record BrazilianState(string Code, string Name, string Region);

public static class BrazilianStates
{
    private static readonly Dictionary<string, BrazilianState> ByCode = new BrazilianState[]
    {
        new("AC", "Acre", "Norte"),
        new("AL", "Alagoas", "Nordeste"),
        new("AP", "Amapá", "Norte"),
        new("AM", "Amazonas", "Norte"),
        new("BA", "Bahia", "Nordeste"),
        new("CE", "Ceará", "Nordeste"),
        new("DF", "Distrito Federal", "Centro-Oeste"),
        new("ES", "Espírito Santo", "Sudeste"),
        new("GO", "Goiás", "Centro-Oeste"),
        new("MA", "Maranhão", "Nordeste"),
        new("MT", "Mato Grosso", "Centro-Oeste"),
        new("MS", "Mato Grosso do Sul", "Centro-Oeste"),
        new("MG", "Minas Gerais", "Sudeste"),
        new("PA", "Pará", "Norte"),
        new("PB", "Paraíba", "Nordeste"),
        new("PR", "Paraná", "Sul"),
        new("PE", "Pernambuco", "Nordeste"),
        new("PI", "Piauí", "Nordeste"),
        new("RJ", "Rio de Janeiro", "Sudeste"),
        new("RN", "Rio Grande do Norte", "Nordeste"),
        new("RS", "Rio Grande do Sul", "Sul"),
        new("RO", "Rondônia", "Norte"),
        new("RR", "Roraima", "Norte"),
        new("SC", "Santa Catarina", "Sul"),
        new("SP", "São Paulo", "Sudeste"),
        new("SE", "Sergipe", "Nordeste"),
        new("TO", "Tocantins", "Norte")
    }.ToDictionary(state => state.Code, StringComparer.OrdinalIgnoreCase);

    public static bool Exists(string? code) =>
        code is not null && ByCode.ContainsKey(code);

    public static BrazilianState? Find(string? code) =>
        code is not null && ByCode.TryGetValue(code, out var state) ? state : null;
}
