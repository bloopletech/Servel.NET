using System.Text;

namespace Servel.NET;

public static class UrlUtility
{
    // Based on https://en.wikipedia.org/wiki/Percent-encoding
    private readonly static Dictionary<char, string> ReservedCharacters = new()
    {
        ['!'] = "%21",
        ['#'] = "%23",
        ['$'] = "%24",
        ['%'] = "%25",
        ['&'] = "%26",
        ['\''] = "%27",
        ['('] = "%28",
        [')'] = "%29",
        ['*'] = "%2A",
        ['+'] = "%2B",
        [','] = "%2C",
        // Intentionally skipping forward slash
        [':'] = "%3A",
        [';'] = "%3B",
        ['='] = "%3D",
        ['?'] = "%3F",
        ['@'] = "%40",
        ['['] = "%5B",
        [']'] = "%5D"
    };

    public static string EncodeUrlPath(string input)
    {
        var output = new StringBuilder(input.Length);
        foreach(var c in input)
        {
            if(ReservedCharacters.TryGetValue(c, out var r)) output.Append(r);
            else output.Append(c);
        }

        return output.ToString();
    }
}
