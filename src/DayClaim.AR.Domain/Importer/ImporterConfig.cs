using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Domain.Importer;

/// <summary>
/// Generic field-to-column mapping per RCM report, per client (deck slide 23:
/// "Data Importer – Configuration"). Deployed centrally; only Importer
/// Storage (the actual data) is per-client.
/// </summary>
public class ImporterConfig : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public RcmReportType RcmReportType { get; set; }
    public ImportSourceType SourceType { get; set; }
    public ImportDataFormat DataFormat { get; set; }
    public ImportScheduleTrigger ScheduleTrigger { get; set; }
    /// <summary>Cron-style expression when ScheduleTrigger is Scheduled; ignored for Reactive.</summary>
    public string? ImportFrequencyCron { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ImporterFieldMapping> FieldMappings { get; set; } = new List<ImporterFieldMapping>();
}

/// <summary>
/// One source-column -> target-field mapping. Full lifecycle + soft-delete
/// history is inherited from AuditableEntity so a changed mapping never loses
/// its prior definition (deck slide 23: "full lifecycle and soft-delete history").
/// </summary>
public class ImporterFieldMapping : AuditableEntity
{
    public Guid ImporterConfigId { get; set; }
    public string SourceColumnName { get; set; } = string.Empty;
    public string TargetFieldName { get; set; } = string.Empty;
    public FieldClassification Classification { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsUniquePrimaryIdentifier { get; set; }
    public bool IsUniqueSecondaryIdentifier { get; set; }
    public bool ContainsPhi { get; set; }
}
