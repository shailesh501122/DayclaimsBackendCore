namespace DayClaim.AR.Application.Common.Authorization;

/// <summary>
/// Central catalog of authorization policy names, referenced by both the
/// policy registration (Infrastructure) and controller [Authorize] attributes
/// (Api), so the two never drift apart. See docs/SECURITY.md for the full
/// RBAC matrix these map to. Role hierarchy: Admin > Manager > Team Leader > User.
/// </summary>
public static class PolicyNames
{
    /// <summary>Any of the four roles — everyone who's an active staff account.</summary>
    public const string InternalStaff = "InternalStaff";

    /// <summary>Admin only — system-wide config, rule engine and WFM CRUD, per-user menu access grants.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Admin or Manager — user/role management. Handlers additionally forbid a
    /// Manager from granting Admin/Manager roles or editing an Admin account.</summary>
    public const string UserManagement = "UserManagement";

    /// <summary>Team Leader and above — approvals, allocation, rollback. Constant name kept
    /// as SupervisorOrAbove for backward compatibility with existing [Authorize] references;
    /// the role string it checks is "Team Leader" (renamed from "Supervisor").</summary>
    public const string SupervisorOrAbove = "SupervisorOrAbove";

    /// <summary>Any authenticated principal — read-only views scoped to their own assignment.</summary>
    public const string AnyAuthenticatedUser = "AnyAuthenticatedUser";
}
