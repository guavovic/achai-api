using System.Text.RegularExpressions;

namespace Achai.Api.Common;

public static partial class StreetNameVariations
{
    private const int MaxVariations = 4;

    private static readonly string[] Romans =
        ["", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV", "XVI",
         "XVII", "XVIII", "XIX", "XX", "XXI", "XXII", "XXIII", "XXIV", "XXV", "XXVI", "XXVII", "XXVIII", "XXIX",
         "XXX", "XXXI", "XXXII", "XXXIII", "XXXIV", "XXXV", "XXXVI", "XXXVII", "XXXVIII", "XXXIX"];

    public static string Clean(string street) =>
        Whitespace().Replace(street.Replace('.', ' '), " ").Trim();

    public static IReadOnlyList<string> For(string street)
    {
        var original = Clean(street);
        var parts = AddressFormatting.SplitStreet(original);
        var expanded = parts.Type is null ? original : $"{parts.Type} {parts.Name}";

        string?[] candidates =
        [
            expanded,
            SwapNumerals(expanded),
            parts.Type is null ? null : parts.Name,
            parts.Type is null ? null : SwapNumerals(parts.Name!),
        ];

        return candidates
            .OfType<string>()
            .Where(candidate => candidate.Length >= 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(candidate => !string.Equals(candidate, original, StringComparison.OrdinalIgnoreCase))
            .Take(MaxVariations)
            .ToList();
    }

    private static string SwapNumerals(string text) =>
        Word().Replace(text, match =>
        {
            var word = match.Value;
            if (int.TryParse(word, out var number))
                return number is > 0 and < 40 ? Romans[number] : word;

            var index = Array.IndexOf(Romans, word.ToUpperInvariant());
            return index > 0 ? index.ToString() : word;
        });

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"\b\w+\b")]
    private static partial Regex Word();
}
