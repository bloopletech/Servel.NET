using System.Security.Cryptography;
using System.Text;

namespace Servel.NET;

// Derived from https://stackoverflow.com/a/1344255
public static class KeyGenerator
{
    private static readonly char[] chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string GetUniqueKey(int size)
    {
        var result = new StringBuilder(size);
        for(int i = 0; i < size; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(0, chars.Length);
            result.Append(chars[idx]);
        }

        return result.ToString();
    }
}