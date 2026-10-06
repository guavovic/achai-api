using Achai.Api.Common;

namespace Achai.Api.Tests.Unit.Common;

public class AddressFormattingTests
{
    [Theory]
    [InlineData("Praça da Sé", "Praça", "da Sé")]
    [InlineData("Rua XV de Novembro", "Rua", "XV de Novembro")]
    [InlineData("Av. Paulista", "Avenida", "Paulista")]
    [InlineData("R. das Flores", "Rua", "das Flores")]
    [InlineData("PCA DA SE", "Praça", "da Se")]
    [InlineData("rua quinze de novembro", "Rua", "Quinze de Novembro")]
    [InlineData("AVENIDA DOM PEDRO II", "Avenida", "Dom Pedro II")]
    [InlineData("Tv.  Padre   Eutíquio", "Travessa", "Padre Eutíquio")]
    public void SplitStreet_SeparatesTheTypeAndExpandsAbbreviations(string street, string type, string name)
    {
        var parts = AddressFormatting.SplitStreet(street);

        parts.Type.ShouldBe(type);
        parts.Name.ShouldBe(name);
    }

    [Theory]
    [InlineData("Boulevard Shopping", null, "Boulevard Shopping")]
    [InlineData("Rua", null, "Rua")]
    [InlineData("", null, null)]
    [InlineData(null, null, null)]
    public void SplitStreet_WithoutAKnownType_KeepsTheWholeName(string? street, string? type, string? name)
    {
        AddressFormatting.SplitStreet(street).ShouldBe(new StreetParts(type, name));
    }

    [Fact]
    public void Format_BuildsOneLineForALabel()
    {
        var address = new Address("01001-000", "Praça da Sé", "lado ímpar", "", "Sé", "São Paulo", "SP", "São Paulo", "Sudeste");

        AddressFormatting.Format(address).ShouldBe("Praça da Sé, lado ímpar - Sé, São Paulo/SP, CEP 01001-000");
    }

    [Fact]
    public void Format_ForTheCityWideZipCode_SkipsTheEmptyParts()
    {
        var address = new Address("89010-971", "", "", "", "", "Blumenau", "SC", "Santa Catarina", "Sul");

        AddressFormatting.Format(address).ShouldBe("Blumenau/SC, CEP 89010-971");
    }
}
