using Achai.Api.Common;

namespace Achai.Api.ContractTests;

/// <summary>
/// Chamam as APIs externas de verdade e conferem se as respostas ainda viram endereços completos.
/// Rodam toda semana pelo workflow "Contrato das APIs externas", não no CI das PRs.
/// </summary>
[Trait("Category", "Contract")]
public class ExternalApiContractTests : IClassFixture<ExternalApis>
{
    private readonly ExternalApis _apis;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public ExternalApiContractTests(ExternalApis apis)
    {
        _apis = apis;
    }

    [Fact(Explicit = true)]
    public async Task ViaCep_KnownZipCode_ReturnsTheFullAddress()
    {
        var result = await _apis.ViaCep.GetByZipCodeAsync("01001000", _ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldSatisfyAllConditions(
            address => address.ZipCode.ShouldBe("01001-000"),
            address => address.Street.ShouldBe("Praça da Sé"),
            address => address.Neighborhood.ShouldBe("Sé"),
            address => address.City.ShouldBe("São Paulo"),
            address => address.State.ShouldBe("SP"),
            address => address.StateName.ShouldBe("São Paulo"),
            address => address.Region.ShouldBe("Sudeste"));
    }

    [Fact(Explicit = true)]
    public async Task ViaCep_UnknownZipCode_ReturnsNotFound()
    {
        var result = await _apis.ViaCep.GetByZipCodeAsync("99999999", _ct);

        result.Error.ShouldBe(AddressErrors.ZipCodeNotFound);
    }

    [Fact(Explicit = true)]
    public async Task ViaCep_StreetSearch_ReturnsAddressesInTheCity()
    {
        var result = await _apis.ViaCep.SearchByStreetAsync("RJ", "Rio de Janeiro", "Atlântica", _ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeEmpty();
        result.Value.ShouldAllBe(address => address.City == "Rio de Janeiro" && address.State == "RJ");
    }

    [Fact(Explicit = true)]
    public async Task BrasilApi_KnownZipCode_ReturnsTheAddressWithStateAndRegion()
    {
        var result = await _apis.BrasilApi.GetByZipCodeAsync("01001000", _ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldSatisfyAllConditions(
            address => address.Street.ShouldBe("Praça da Sé"),
            address => address.City.ShouldBe("São Paulo"),
            address => address.State.ShouldBe("SP"),
            address => address.StateName.ShouldBe("São Paulo"),
            address => address.Region.ShouldBe("Sudeste"));
    }

    [Fact(Explicit = true)]
    public async Task BrasilApi_UnknownZipCode_ReturnsNotFound()
    {
        var result = await _apis.BrasilApi.GetByZipCodeAsync("00000000", _ct);

        result.Error.ShouldBe(AddressErrors.ZipCodeNotFound);
    }

    [Fact(Explicit = true)]
    public async Task Ibge_CitiesOfAState_ReturnsTheWholeList()
    {
        var result = await _apis.Ibge.GetByStateAsync("SC", _ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBeGreaterThan(250);
        result.Value.ShouldContain(city => city.Name == "Florianópolis");
    }
}
