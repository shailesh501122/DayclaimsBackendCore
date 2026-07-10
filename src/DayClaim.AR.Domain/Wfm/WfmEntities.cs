using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Domain.Wfm;

/// <summary>WFM CRUD (team + allocation rule definitions) is centralized; allocation runs per client.</summary>
public class Team : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TeamLeaderUserId { get; set; }
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
}

public class TeamMember
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid UserId { get; set; }
    /// <summary>Used by auto-allocation to weight distribution toward experienced users (deck slide 33).</summary>
    public bool IsExperienced { get; set; }
}

/// <summary>Defines how claims in a bucket/priority get distributed to a team (deck: "Equal Distribution & Location based Allocation").</summary>
public class WfmAllocationRule : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public Guid TeamId { get; set; }
    public ClaimBucket TargetBucket { get; set; }
    public ClaimPriority? TargetPriority { get; set; }
    public AllocationType AllocationType { get; set; }
    public bool EqualDistribution { get; set; } = true;
    public bool LocationBased { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>One claim's assignment to a user; rollback is a soft state change, never a delete (deck: "Roll Back").</summary>
public class Allocation : AuditableEntity
{
    public Guid RcmReportEntryId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public Guid? AssignedByUserId { get; set; }
    public AllocationType AllocationType { get; set; }
    public DateTimeOffset AllocatedAtUtc { get; set; }
    public DateTimeOffset? RolledBackAtUtc { get; set; }
    public string? RollbackReason { get; set; }
}
