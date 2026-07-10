using DayClaim.AR.Domain.Common;

namespace DayClaim.AR.Domain.Notes;

/// <summary>Status > Action > Sub-Action mapping with a note template (matches the existing frontend's Scenario Master module).</summary>
public class ScenarioMaster : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StatusActionSubActionMapping { get; set; } = string.Empty;
    public string NoteTemplate { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ClaimNote : AuditableEntity
{
    public Guid RcmReportEntryId { get; set; }
    public Guid? ScenarioMasterId { get; set; }
    public Guid UserId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
    public string? StatusSet { get; set; }
}
