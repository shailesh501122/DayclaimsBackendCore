using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Application.Features.RuleEngine;

public record RuleDto(
    Guid Id,
    string Name,
    RuleScope Scope,
    Guid? ClientOrganizationId,
    Guid? PayerMasterId,
    string ConditionExpression,
    ClaimBucket ResultBucket,
    ClaimPriority? ResultPriority,
    bool ExcludeSpecialProjectClaims,
    bool ExcludeManualAssignmentClaims,
    int EvaluationOrder,
    bool IsActive);

public record RuleExecutionSummaryDto(
    Guid RunId,
    Guid ClientOrganizationId,
    int ClaimsProcessed,
    int ClaimsMatched,
    string Status,
    IReadOnlyDictionary<string, int> ByBucket);
