using DayClaim.AR.Domain.Common;

namespace DayClaim.AR.Domain.Identity;

/// <summary>
/// Single global user store shared by AR, EVBV and Coding (deck slide 22:
/// "Centralized User Management & RBAC"). Password hashing is a modern
/// salted algorithm (see Infrastructure/Security/PasswordHasher), replacing
/// the legacy MD5+3DES scheme flagged as a security risk in the deck.
/// </summary>
public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Null for internal staff; set for a client-org user.</summary>
    public Guid? PrimaryClientOrganizationId { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }
    public DateTimeOffset? ClosedAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

/// <summary>A user can be tied to one or many client organizations (deck slide 22).</summary>
public class UserOrganization
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ClientOrganizationId { get; set; }
}

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    /// <summary>JWT ID (jti) claim of the access token this refresh token was issued alongside.</summary>
    public string Jti { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public string? CreatedByIp { get; set; }
}
