using System.Text;

namespace Servel.NET;

public static class UrlUtility
{
    public static string EncodeUrlPath(string input)
    {
        var output = new StringBuilder(input.Length);

        foreach(var c in input)
        {
            // Based on https://en.wikipedia.org/wiki/Percent-encoding
            var r = c switch
            {
                '!' => "%21",
                '#' => "%23",
                '$' => "%24",
                '%' => "%25",
                '&' => "%26",
                '\'' => "%27",
                '(' => "%28",
                ')' => "%29",
                '*' => "%2A",
                '+' => "%2B",
                ',' => "%2C",
                // Intentionally skipping forward slash
                ':' => "%3A",
                ';' => "%3B",
                '=' => "%3D",
                '?' => "%3F",
                '@' => "%40",
                '[' => "%5B",
                ']' => "%5D",
                _ => null,
            };

            if(r != null) output.Append(r);
            else output.Append(c);
        }

        return output.ToString();
    }
}
