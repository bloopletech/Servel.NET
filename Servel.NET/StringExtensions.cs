using System.Security.Cryptography;
using System.Text;

namespace Servel.NET;

public static class StringExtensions
{
    public static bool FixedTimeEquals(this string left, string right)
    {
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));
    }
}
