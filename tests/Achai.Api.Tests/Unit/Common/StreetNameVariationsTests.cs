using Achai.Api.Common;

namespace Achai.Api.Tests.Unit.Common;

public class StreetNameVariationsTests
{
    [Fact]
    public void For_ExpandsTheAbbreviation()
    {
        StreetNameVariations.For("Av Paulista").ShouldBe(["Avenida Paulista", "Paulista"]);
    }

    [Fact]
    public void For_SwapsRomanAndArabicNumerals()
    {
        StreetNameVariations.For("XV de Novembro").ShouldBe(["15 de Novembro"]);
        StreetNameVariations.For("15 de Novembro").ShouldBe(["XV de Novembro"]);
    }

    [Fact]
    public void For_TriesTheNameWithoutTheType()
    {
        StreetNameVariations.For("Rua XV de Novembro").ShouldBe(["Rua 15 de Novembro", "XV de Novembro", "15 de Novembro"]);
    }

    [Fact]
    public void For_IgnoresDotsAndExtraSpaces()
    {
        StreetNameVariations.For("Pça.  da Sé").ShouldBe(["Praça da Sé", "da Sé"]);
    }

    [Fact]
    public void For_WithNothingToTry_ReturnsEmpty()
    {
        StreetNameVariations.For("Paulista").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Av. Paulista", "Av Paulista")]
    [InlineData("  Rua   das  Flores ", "Rua das Flores")]
    public void Clean_RemovesDotsAndExtraSpaces(string street, string cleaned)
    {
        StreetNameVariations.Clean(street).ShouldBe(cleaned);
    }
}
