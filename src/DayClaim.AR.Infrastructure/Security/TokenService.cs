using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DayClaim.AR.Infrastructure.Security;

/// <summary>
/// Issues short-lived JWT access tokens (HMAC-SHA256) plus a long-lived,
/// opaque, rotate-on-use refresh token — the SSO design in deck slide 22
/// ("JWT signed via MAC..., replaces today's session+cookie pattern").
/// Authorization is enforced per-endpoint by ASP.NET Core policies reading
/// the role/org claims from this token (see Api/Authorization), closing the
/// "hidden URL still reachable" broken-access-control gap called out in the
/// deck (slide 10, OWASP A01:2021).
/// </summary>
public class TokenService(IOptions<JwtSettings> options) : ITokenService
{
    private readonly JwtSettings _settings = options.Value;

    public TokenPair IssueTokens(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<Guid> clientOrganizationIds)
    {
        if (string.IsNullOrWhiteSpace(_settings.SigningKey) || _settings.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured and at least 32 characters (256 bits).");
        }

        var jti = Guid.NewGuid().ToString("N");
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new("display_name", user.DisplayName),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(clientOrganizationIds.Select(orgId => new Claim("org", orgId.ToString())));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenPair(accessToken, refreshToken, expires, jti);
    }

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
