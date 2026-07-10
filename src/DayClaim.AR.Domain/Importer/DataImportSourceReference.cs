using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Domain.Importer;

/// <summary>
/// One row per received file, tracked regardless of RCM report type (deck
/// slide 24: "DB table tracks every received file"). The file bytes
/// themselves live in object storage (S3 in the target AWS design); this
/// row is the checksum/audit trail.
/// </summary>
public class DataImportSourceReference : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public Guid ImporterConfigId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ObjectStorageKey { get; set; } = string.Empty;
    public string Sha256Checksum { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public int RowCount { get; set; }
    public DataQualityStatus OverallStatus { get; set; }
    public bool WasFallbackWebUpload { get; set; }
}
