using Achai.Api.Common.Validation;

namespace Achai.Api.Tests.Unit.Common;

public class BrazilianStateAttributeTests
{
    private readonly BrazilianStateAttribute _attribute = new();

    [Theory]
    [InlineData("SP")]
    [InlineData("sp")]
    [InlineData("Rj")]
    [InlineData("DF")]
    [InlineData("TO")]
    public void IsValid_WithExistingCode_Accepts(string state)
    {
        _attribute.IsValid(state).ShouldBeTrue();
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("SPP")]
    [InlineData("S")]
    [InlineData("")]
    [InlineData(" SP")]
    public void IsValid_WithUnknownCode_Rejects(string state)
    {
        _attribute.IsValid(state).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNull_Rejects()
    {
        _attribute.IsValid(null).ShouldBeFalse();
    }
}
