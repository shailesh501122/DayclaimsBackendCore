using DayClaim.AR.Domain.Common;

namespace DayClaim.AR.Domain.Identity;

public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsClientRole { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>Fine-grained, per-API-endpoint permission code, e.g. "ar.rule-engine.execute".</summary>
public class Permission : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
