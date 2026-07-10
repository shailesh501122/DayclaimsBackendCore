using DayClaim.AR.Application.Common.Interfaces;
using BCrypt.Net;

namespace DayClaim.AR.Infrastructure.Security;

/// <summary>
/// bcrypt (work factor 12) — replaces the legacy MD5+3DES scheme flagged in
/// the modernization deck (slide 10) as a HIPAA/OWASP risk. bcrypt is
/// intentionally slow and salts automatically per-hash.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plaintextPassword) => BCrypt.Net.BCrypt.HashPassword(plaintextPassword, WorkFactor);

    public bool Verify(string plaintextPassword, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plaintextPassword, hash);
        }
        catch (SaltParseException)
        {
            // Hash was produced by a different/legacy scheme (e.g. not-yet-migrated MD5+3DES row) — treat as no match.
            return false;
        }
    }
}
