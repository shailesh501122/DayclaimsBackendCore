using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Domain.RuleEngine;

/// <summary>
/// Rule Engine CRUD is centralized (deck slide 12); execution runs per
/// client against that client's Importer Storage. A rule classifies a claim
/// into a <see cref="ClaimBucket"/> and, for Workable claims, a
/// <see cref="ClaimPriority"/>.
/// </summary>
public class Rule : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public RuleScope Scope { get; set; }
    public Guid? ClientOrganizationId { get; set; }
    public Guid? PayerMasterId { get; set; }

    /// <summary>Small, safe boolean-expression DSL evaluated against the claim's JSON fields, e.g. "balance > 100 AND ageing_bucket == '91-120'".</summary>
    public string ConditionExpression { get; set; } = string.Empty;

    public ClaimBucket ResultBucket { get; set; }
    public ClaimPriority? ResultPriority { get; set; }

    /// <summary>Deck slide 6: rule execution "excludes Special Project & Manual Claims Assignment" and "excludes all standard Rules" for best-case scenario claims.</summary>
    public bool ExcludeSpecialProjectClaims { get; set; }
    public bool ExcludeManualAssignmentClaims { get; set; }

    public int EvaluationOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>One rule-engine execution batch for a client (deck: "Rule Engine Summary/Report/Dashboard").</summary>
public class RuleExecutionRun : AuditableEntity
{
    public Guid ClientOrganizationId { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public int ClaimsProcessed { get; set; }
    public int ClaimsMatched { get; set; }
    public string Status { get; set; } = "running";
}

public class RuleExecutionResult : AuditableEntity
{
    public Guid RuleExecutionRunId { get; set; }
    public Guid RcmReportEntryId { get; set; }
    public Guid RuleId { get; set; }
    public ClaimBucket Bucket { get; set; }
    public ClaimPriority? Priority { get; set; }
}
