using DayClaim.AR.Domain.Enums;

namespace DayClaim.AR.Application.Features.Wfm;

public record TeamDto(Guid Id, string Name, Guid TeamLeaderUserId, IReadOnlyCollection<Guid> MemberUserIds);

public record AllocationRuleDto(
    Guid Id, Guid TeamId, ClaimBucket TargetBucket, ClaimPriority? TargetPriority,
    AllocationType AllocationType, bool EqualDistribution, bool LocationBased, bool IsActive);

public record AllocationResultSummaryDto(int ClaimsAllocated, IReadOnlyDictionary<string, int> ByUser);

public record AllocationDto(Guid Id, Guid RcmReportEntryId, Guid AssignedToUserId, AllocationType AllocationType, DateTimeOffset AllocatedAtUtc);
