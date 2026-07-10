namespace DayClaim.AR.Application.Common.Authorization;

/// <summary>
/// Central catalog of authorization policy names, referenced by both the
/// policy registration (Infrastructure) and controller [Authorize] attributes
/// (Api), so the two never drift apart. See docs/SECURITY.md for the full
/// RBAC matrix these map to.
/// </summary>
public static class PolicyNames
{
    /// <summary>Any internal-staff role (Super Admin, Site Admin, Supervisor, User) — excludes client-org users.</summary>
    public const string InternalStaff = "InternalStaff";

    /// <summary>Super Admin or Site Admin only — user/role management, rule engine and WFM CRUD.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Supervisor and above — approvals, allocation, rollback.</summary>
    public const string SupervisorOrAbove = "SupervisorOrAbove";

    /// <summary>Any authenticated principal, internal or client-org — read-only views scoped to their own org.</summary>
    public const string AnyAuthenticatedUser = "AnyAuthenticatedUser";
}
