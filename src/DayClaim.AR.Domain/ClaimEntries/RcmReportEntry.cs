using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Domain.ClaimEntries;

/// <summary>
/// The central AR aggregate: one imported claim/record for a given RCM
/// report type (Ageing/Denials/Charges/Payments/Adjustments). Field storage
/// is segregated into four JSONB sections exactly as described in deck
/// slide 24 ("Standard fields / Customer-specific / Custom-defined /
/// Status-storing") plus an "unmapped" catch-all — this is what lets the
/// same table serve every client's customised report layout without a
/// schema migration per client.
///
/// This is the write-model (event-sourced: insert + select only, per deck
/// slide 26). Downstream sync jobs project approved/re-opened entries into
/// read-model tables per report type; that projection is out of scope for
/// this first cut and is called out in docs/ARCHITECTURE.md.
/// </summary>
public class RcmReportEntry : AuditableEntity, IClientScoped
{
    public Guid ClientOrganizationId { get; set; }
    public RcmReportType RcmReportType { get; set; }
    public Guid DataImportSourceReferenceId { get; set; }
    public Guid PatientMasterId { get; set; }
    public Guid EnterprisePatientIndexId { get; set; }

    /// <summary>Well-known columns every client has for this report type (JSONB).</summary>
    public string StandardFieldsJson { get; set; } = "{}";
    /// <summary>Fields specific to this client but still modeled (JSONB).</summary>
    public string CustomerSpecificFieldsJson { get; set; } = "{}";
    /// <summary>Ad-hoc/derived fields defined by the client (JSONB).</summary>
    public string CustomDefinedFieldsJson { get; set; } = "{}";
    /// <summary>Workflow status fields (closing, reopened, ...) (JSONB).</summary>
    public string StatusStoringFieldsJson { get; set; } = "{}";
    /// <summary>Anything present in the source file with no configured mapping (JSONB).</summary>
    public string UnmappedFieldsJson { get; set; } = "{}";

    public EncryptedValue Balance { get; set; } = EncryptedValue.Empty;

    public DataQualityStatus Status { get; set; }
    /// <summary>Set by Rule Engine execution; null until a rule run classifies the entry.</summary>
    public ClaimBucket? Bucket { get; set; }
    public ClaimPriority? Priority { get; set; }

    /// <summary>UUIDv7 of the earliest entry for this same claim, so re-imports link back to history instead of duplicating (deck slide 26, replaces "duplicate_blank_status" flag).</summary>
    public Guid? OriginalEntryId { get; set; }

    public DateTimeOffset OpenedAtUtc { get; set; }
    public DateTimeOffset? ClosedAtUtc { get; set; }
}

/// <summary>Event log driving the read-model sync (deck slide 26, step 8: "write event to ...sync_requests").</summary>
public class RcmReportDataEntrySyncRequest : AuditableEntity
{
    public Guid RcmReportEntryId { get; set; }
    public DateTimeOffset RequestedAtUtc { get; set; }
    public DateTimeOffset? SyncedAtUtc { get; set; }
    public string Status { get; set; } = "pending";
}

/// <summary>Date-wise SLA snapshot for ageing entries (deck slide 24: "rd_ageing_data_*_sla_history").</summary>
public class AgeingSlaHistory : AuditableEntity
{
    public Guid RcmReportEntryId { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public int AgeInDays { get; set; }
    public string Bucket { get; set; } = string.Empty; // 0-30 / 31-60 / 61-90 / 91-120 / 120+
}
