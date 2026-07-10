using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Domain.Identity;

/// <summary>
/// A client organization (provider / hospital / billing department). Drives
/// downstream per-client schema and infrastructure-sizing decisions
/// (deck slide 21: "Org_id in users DB drives downstream config").
/// </summary>
public class ClientOrganization : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ClientOrgSize Size { get; set; }
    public bool IsActive { get; set; } = true;
}
