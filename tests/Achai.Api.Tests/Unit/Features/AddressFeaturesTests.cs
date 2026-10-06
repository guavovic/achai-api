using Achai.Api.Common;
using Achai.Api.Common.Results;
using Achai.Api.Features.Addresses;
using Achai.Api.Features.Cities;
using Achai.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Achai.Api.Tests.Unit.Features;

public class AddressFeaturesTests
{
    private static readonly Address PracaDaSe = new(
        "01001-000", "Praça da Sé", "lado ímpar", "", "Sé", "São Paulo", "SP", "São Paulo", "Sudeste");

    private readonly IAddressProvider _addressProvider = Substitute.For<IAddressProvider>();
    private readonly ICityProvider _cityProvider = Substitute.For<ICityProvider>();
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetAddressByZipCode_RemovesTheHyphenBeforeSearching()
    {
        _addressProvider.GetByZipCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(PracaDaSe);

        await GetAddressByZipCode.HandleAsync("01001-000", _addressProvider, _ct);

        await _addressProvider.Received(1).GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAddressByZipCode_WhenFound_MapsEveryField()
    {
        _addressProvider.GetByZipCodeAsync("01001000", Arg.Any<CancellationToken>()).Returns(PracaDaSe);

        var result = await GetAddressByZipCode.HandleAsync("01001000", _addressProvider, _ct);

        var response = result.ShouldBeOfType<Ok<AddressResponse>>().Value!;
        response.ShouldSatisfyAllConditions(
            r => r.ZipCode.ShouldBe("01001-000"),
            r => r.Street.ShouldBe("Praça da Sé"),
            r => r.Complement.ShouldBe("lado ímpar"),
            r => r.Unit.ShouldBe(""),
            r => r.Neighborhood.ShouldBe("Sé"),
            r => r.City.ShouldBe("São Paulo"),
            r => r.State.ShouldBe("SP"),
            r => r.StateName.ShouldBe("São Paulo"),
            r => r.Region.ShouldBe("Sudeste"));
    }

    [Fact]
    public async Task GetAddressByZipCode_WhenProviderReturnsAnError_ReturnsProblem()
    {
        _addressProvider.GetByZipCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(AddressErrors.ZipCodeNotFound);

        var result = await GetAddressByZipCode.HandleAsync("99999999", _addressProvider, _ct);

        var problem = result.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(404);
        problem.ProblemDetails.Extensions["code"].ShouldBe("Endereco.CepNaoEncontrado");
    }

    [Fact]
    public async Task SearchAddressesByStreet_SendsTheStateInUpperCase()
    {
        _addressProvider.SearchByStreetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new List<Address>());

        await SearchAddressesByStreet.HandleAsync("sp", "São Paulo", "Paulista", _addressProvider, new DefaultHttpContext(), _ct);

        await _addressProvider.Received(1).SearchByStreetAsync("SP", "São Paulo", "Paulista", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCitiesByState_SendsTheStateInUpperCaseAndMapsTheNames()
    {
        _cityProvider.GetByStateAsync("AC", Arg.Any<CancellationToken>())
            .Returns(new List<City> { new("Acrelândia"), new("Assis Brasil") });

        var result = await GetCitiesByState.HandleAsync("ac", _cityProvider, _ct);

        var cities = result.ShouldBeOfType<Ok<List<CityResponse>>>().Value!;
        cities.Select(c => c.Name).ShouldBe(["Acrelândia", "Assis Brasil"]);
    }
}
