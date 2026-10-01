using Achai.Api.Common;

namespace Achai.Api.Tests.Unit.Common;

public class BrazilianStatesTests
{
    [Theory]
    [InlineData("SP", "São Paulo", "Sudeste")]
    [InlineData("df", "Distrito Federal", "Centro-Oeste")]
    [InlineData("AM", "Amazonas", "Norte")]
    [InlineData("RS", "Rio Grande do Sul", "Sul")]
    [InlineData("BA", "Bahia", "Nordeste")]
    public void Find_ReturnsNameAndRegion(string code, string name, string region)
    {
        var state = BrazilianStates.Find(code);

        state.ShouldNotBeNull();
        state.Name.ShouldBe(name);
        state.Region.ShouldBe(region);
    }

    [Fact]
    public void Find_WithUnknownCode_ReturnsNull()
    {
        BrazilianStates.Find("XX").ShouldBeNull();
    }
}
