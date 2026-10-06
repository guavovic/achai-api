using System.Text;

namespace Achai.Api.Common;

public static class TextFolding
{
    private const string Accented = "áàâãäéèêëíìîïóòôõöúùûüçñÁÀÂÃÄÉÈÊËÍÌÎÏÓÒÔÕÖÚÙÛÜÇÑ";
    private const string Plain = "aaaaaeeeeiiiiooooouuuucnAAAAAEEEEIIIIOOOOOUUUUCN";

    public static string Fold(string? text)
    {
        var builder = new StringBuilder((text ?? "").Trim().ToLowerInvariant());
        for (var i = 0; i < builder.Length; i++)
        {
            var index = Accented.IndexOf(builder[i]);
            if (index >= 0)
                builder[i] = Plain[index];
        }

        return builder.ToString();
    }
}
