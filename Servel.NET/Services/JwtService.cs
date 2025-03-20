using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Servel.NET.Services;

public class JwtService
{
    private static readonly TimeSpan MaxSameFileDuration = new(12, 0, 0);
    private const string Issuer = "Servel.NET";
    private const string PathClaimType = "path";
    private const string SiteIdClaimType = "site_id";

    private readonly string _siteId;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtService(Site site)
    {
        _siteId = site.Id.ToString();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(site.JwtSigningKey!));
        _signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateAudience = false,
            ValidIssuer = Issuer
        };
    }

    public string Generate()
    {
        return GenerateToken([]);
    }

    public string Generate(string path)
    {
        return GenerateToken([new Claim(PathClaimType, path)]);
    }

    private string GenerateToken(IEnumerable<Claim> claims)
    {
        var securityToken = new JwtSecurityToken(
            issuer: Issuer,
            expires: DateTime.Now.Add(MaxSameFileDuration),
            signingCredentials: _signingCredentials,
            claims: [new Claim(SiteIdClaimType, _siteId), ..claims]);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    public bool Validate(string token)
    {
        return ParseToken(token) != null;
    }

    public bool Validate(string token, string path)
    {
        var claims = ParseToken(token);
        if(claims == null) return false;

        var siteIdClaim = claims.FirstOrDefault(c => c.Type == SiteIdClaimType);
        var pathClaim = claims.FirstOrDefault(c => c.Type == PathClaimType);
        return siteIdClaim?.Value == _siteId && pathClaim?.Value == path;
    }

    private IEnumerable<Claim>? ParseToken(string token)
    {
        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, _tokenValidationParameters, out var securityToken);

            if(securityToken is JwtSecurityToken jwtSecurityToken)
            {
                return jwtSecurityToken.Claims;
            }
        }
        catch(SecurityTokenException)
        {
        }

        return null;
    }
}
