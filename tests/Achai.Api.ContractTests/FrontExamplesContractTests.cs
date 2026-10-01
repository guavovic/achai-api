using System.Text.Json;

namespace Achai.Api.ContractTests;

/// <summary>
/// Os exemplos que o front sorteia no estado vazio (web/src/app/search/examples.json)
/// precisam continuar trazendo resultado: um exemplo clicável que volta vazio passa impressão de defeito.
/// </summary>
[Trait("Category", "Contract")]
public class FrontExamplesContractTests : IClassFixture<ExternalApis>
{
    private readonly ExternalApis _apis;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public FrontExamplesContractTests(ExternalApis apis)
    {
        _apis = apis;
    }

    public static TheoryData<string, string> ZipCodeExamples() =>
        new(FrontExamples.Load().ZipCodes.Select(example => (example.ZipCode, example.Place)));

    public static TheoryData<string, string, string> StreetExamples() =>
        new(FrontExamples.Load().Streets.Select(example => (example.State, example.City, example.Street)));

    [Theory(Explicit = true)]
    [MemberData(nameof(ZipCodeExamples))]
    public async Task ZipCodeExample_Exists(string zipCode, string place)
    {
        var result = await _apis.ViaCep.GetByZipCodeAsync(zipCode.Replace("-", ""), _ct);

        result.IsSuccess.ShouldBeTrue($"O exemplo {zipCode} ({place}) não foi encontrado no ViaCEP.");
    }

    [Theory(Explicit = true)]
    [MemberData(nameof(StreetExamples))]
    public async Task StreetExample_HasResults(string state, string city, string street)
    {
        var result = await _apis.ViaCep.SearchByStreetAsync(state, city, street, _ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeEmpty($"A busca por \"{street}\" em {city}/{state} não trouxe resultado.");
    }
}

internal sealed record FrontExamples(List<ZipCodeExample> ZipCodes, List<StreetExample> Streets)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static FrontExamples Load()
    {
        var path = Path.Combine(FindRepositoryRoot(), "web", "src", "app", "search", "examples.json");
        return JsonSerializer.Deserialize<FrontExamples>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidOperationException($"Não foi possível ler {path}.");
    }

    // Sobe a partir da pasta do teste até achar a solução, que fica na raiz do repositório.
    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (directory.GetFiles("Achai.slnx").Length > 0)
                return directory.FullName;
        }

        throw new InvalidOperationException("Raiz do repositório (Achai.slnx) não encontrada.");
    }
}

internal sealed record ZipCodeExample(string ZipCode, string Place);

internal sealed record StreetExample(string State, string City, string Street);
