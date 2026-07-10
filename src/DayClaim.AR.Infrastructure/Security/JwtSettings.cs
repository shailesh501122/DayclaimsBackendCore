namespace DayClaim.AR.Infrastructure.Security;

/// <summary>
/// Bound from configuration (appsettings / environment / — in production —
/// AWS Secrets Manager per the modernization deck). SigningKey must be at
/// least 256 bits; there is no fallback default so a misconfigured
/// deployment fails fast at startup instead of silently using a weak key.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
