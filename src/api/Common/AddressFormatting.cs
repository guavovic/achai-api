using System.Globalization;
using System.Text.RegularExpressions;

namespace Achai.Api.Common;

public sealed record StreetParts(string? Type, string? Name);

public static partial class AddressFormatting
{
    private static readonly Dictionary<string, string> StreetTypes = new()
    {
        ["rua"] = "Rua", ["r"] = "Rua",
        ["avenida"] = "Avenida", ["av"] = "Avenida",
        ["praca"] = "Praça", ["pc"] = "Praça", ["pca"] = "Praça",
        ["travessa"] = "Travessa", ["tv"] = "Travessa", ["trav"] = "Travessa",
        ["alameda"] = "Alameda", ["al"] = "Alameda",
        ["rodovia"] = "Rodovia", ["rod"] = "Rodovia",
        ["estrada"] = "Estrada", ["estr"] = "Estrada",
        ["largo"] = "Largo", ["lgo"] = "Largo",
        ["ladeira"] = "Ladeira", ["lad"] = "Ladeira",
        ["viela"] = "Viela",
        ["beco"] = "Beco",
        ["servidao"] = "Servidão",
        ["passagem"] = "Passagem",
        ["via"] = "Via",
        ["vila"] = "Vila",
        ["quadra"] = "Quadra",
        ["parque"] = "Parque",
    };

    private static readonly HashSet<string> LowercaseWords = ["a", "da", "das", "de", "do", "dos", "e", "em", "na", "no"];

    public static StreetParts SplitStreet(string? street)
    {
        if (string.IsNullOrWhiteSpace(street))
            return new(null, null);

        var text = NormalizeCase(Whitespace().Replace(street.Trim(), " "));
        var space = text.IndexOf(' ');
        if (space > 0 && StreetTypes.TryGetValue(Key(text[..space]), out var type))
            return new(type, text[(space + 1)..]);

        return new(null, text);
    }

    public static string Format(Address address)
    {
        var (type, name) = SplitStreet(address.Street);
        var street = Join(", ", Join(" ", type, name), address.Complement);
        var city = Join("/", address.City, address.State);

        return Join(", ", Join(" - ", street, address.Neighborhood), city, $"CEP {address.ZipCode}");
    }

    private static string NormalizeCase(string text)
    {
        var hasUpper = text.Any(char.IsUpper);
        var hasLower = text.Any(char.IsLower);
        if (hasUpper && hasLower)
            return text;

        var words = text.ToLowerInvariant().Split(' ');
        for (var i = 0; i < words.Length; i++)
        {
            if (RomanNumeral().IsMatch(words[i]))
                words[i] = words[i].ToUpperInvariant();
            else if (i == 0 || !LowercaseWords.Contains(words[i]))
                words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i]);
        }

        return string.Join(' ', words);
    }

    private static string Key(string word) => TextFolding.Fold(word.TrimEnd('.'));

    private static string Join(string separator, params string?[] parts) =>
        string.Join(separator, parts.Where(part => !string.IsNullOrWhiteSpace(part)));

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex("^(?=[ivxlc]+$)c{0,3}(xc|xl|l?x{0,3})(ix|iv|v?i{0,3})$")]
    private static partial Regex RomanNumeral();
}
