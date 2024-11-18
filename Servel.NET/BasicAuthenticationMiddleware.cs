using System.Security.Cryptography;
using System.Text;

namespace Servel.NET;

// Derived from https://github.com/blowdart/idunno.Authentication/blob/8594ba81d5fa7cf5b473d728c355077dd9b3eaea/src/idunno.Authentication.Basic/BasicAuthenticationHandler.cs
public class BasicAuthenticationMiddleware(RequestDelegate next, Credentials credentials)
{
    public const string Scheme = "Basic";
    public const byte Delimiter = 0x3A; // U+003A COLON character (:)
    private readonly byte[] _username = Encoding.UTF8.GetBytes(credentials.Username);
    private readonly byte[] _password = Encoding.UTF8.GetBytes(credentials.Password);

    public async Task Invoke(HttpContext httpContext)
    {
        if (IsAuthenticated(httpContext.Request.Headers.Authorization.FirstOrDefault()))
        {
            await next.Invoke(httpContext);
        }
        else
        {
            httpContext.Response.Headers.WWWAuthenticate = $"{Scheme} realm=\"Servel.NET\", charset=\"UTF-8\"";
            await Results.Unauthorized().ExecuteAsync(httpContext);
        }
    }

    private bool IsAuthenticated(string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader)) return false;

        // Exact match on purpose, rather than using string compare
        // asp.net request parsing will always trim the header and remove trailing spaces
        if (Scheme == authorizationHeader) return false;
 
        if (!authorizationHeader.StartsWith($"{Scheme} ", StringComparison.OrdinalIgnoreCase)) return false;

        var encodedCredentials = authorizationHeader[Scheme.Length..].Trim();

        byte[] decodedCredentials;
        try
        {
            decodedCredentials = Convert.FromBase64String(encodedCredentials);
        }
        catch (FormatException)
        {
            return false;
        }

        var delimiterIndex = Array.IndexOf(decodedCredentials, Delimiter);
        if (delimiterIndex == -1) return false;

        var username = decodedCredentials[0..delimiterIndex];
        var password = decodedCredentials[(delimiterIndex + 1)..];

        return CryptographicOperations.FixedTimeEquals(_username, username) & CryptographicOperations.FixedTimeEquals(_password, password);
    }
}