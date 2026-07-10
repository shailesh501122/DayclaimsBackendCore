using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Dashboard;

public record InventorySummaryDto(
    int TotalOpen,
    IReadOnlyDictionary<string, int> ByBucket,
    IReadOnlyDictionary<string, int> ByStatus,
    double AverageAgeDays);

/// <summary>Backs the frontend's "Inventory Summary / Report / Dashboard" modules.</summary>
public record GetInventorySummaryQuery(Guid ClientOrganizationId) : IRequest<InventorySummaryDto>;

public class GetInventorySummaryQueryHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IRequestHandler<GetInventorySummaryQuery, InventorySummaryDto>
{
    public async Task<InventorySummaryDto> Handle(GetInventorySummaryQuery request, CancellationToken cancellationToken)
    {
        var openEntries = await db.RcmReportEntries
            .Where(e => e.ClientOrganizationId == request.ClientOrganizationId
                        && e.Status != DataQualityStatus.Closed && e.Status != DataQualityStatus.Returned && !e.IsDeleted)
            .Select(e => new { e.Bucket, e.Status, e.OpenedAtUtc })
            .ToListAsync(cancellationToken);

        var byBucket = openEntries
            .GroupBy(e => e.Bucket?.ToString() ?? "Unclassified")
            .ToDictionary(g => g.Key, g => g.Count());

        var byStatus = openEntries
            .GroupBy(e => e.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var avgAge = openEntries.Count == 0
            ? 0
            : openEntries.Average(e => (clock.UtcNow - e.OpenedAtUtc).TotalDays);

        return new InventorySummaryDto(openEntries.Count, byBucket, byStatus, Math.Round(avgAge, 1));
    }
}

public record AssignmentSummaryDto(int TotalAllocated, int TotalRolledBack, IReadOnlyDictionary<string, int> ByUser);

/// <summary>Backs the frontend's "Assignment Summary / Report / Dashboard" modules.</summary>
public record GetAssignmentSummaryQuery(Guid ClientOrganizationId) : IRequest<AssignmentSummaryDto>;

public class GetAssignmentSummaryQueryHandler(IApplicationDbContext db) : IRequestHandler<GetAssignmentSummaryQuery, AssignmentSummaryDto>
{
    public async Task<AssignmentSummaryDto> Handle(GetAssignmentSummaryQuery request, CancellationToken cancellationToken)
    {
        var allocations = await db.Allocations
            .Join(db.RcmReportEntries, a => a.RcmReportEntryId, e => e.Id, (a, e) => new { a, e })
            .Where(x => x.e.ClientOrganizationId == request.ClientOrganizationId)
            .Select(x => new { x.a.AssignedToUserId, x.a.RolledBackAtUtc })
            .ToListAsync(cancellationToken);

        var active = allocations.Where(a => a.RolledBackAtUtc == null).ToList();
        var byUser = active
            .GroupBy(a => a.AssignedToUserId.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new AssignmentSummaryDto(active.Count, allocations.Count - active.Count, byUser);
    }
}

public record RuleEngineSummaryDto(Guid? LastRunId, DateTimeOffset? LastRunAtUtc, int LastRunClaimsProcessed, int LastRunClaimsMatched, IReadOnlyDictionary<string, int> ByBucket);

/// <summary>Backs the frontend's "Rule Engine Summary / Report / Dashboard" modules.</summary>
public record GetRuleEngineSummaryQuery(Guid ClientOrganizationId) : IRequest<RuleEngineSummaryDto>;

public class GetRuleEngineSummaryQueryHandler(IApplicationDbContext db) : IRequestHandler<GetRuleEngineSummaryQuery, RuleEngineSummaryDto>
{
    public async Task<RuleEngineSummaryDto> Handle(GetRuleEngineSummaryQuery request, CancellationToken cancellationToken)
    {
        var lastRun = await db.RuleExecutionRuns
            .Where(r => r.ClientOrganizationId == request.ClientOrganizationId)
            .OrderByDescending(r => r.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastRun is null)
        {
            return new RuleEngineSummaryDto(null, null, 0, 0, new Dictionary<string, int>());
        }

        var byBucket = await db.RuleExecutionResults
            .Where(r => r.RuleExecutionRunId == lastRun.Id)
            .GroupBy(r => r.Bucket)
            .Select(g => new { Bucket = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new RuleEngineSummaryDto(
            lastRun.Id, lastRun.StartedAtUtc, lastRun.ClaimsProcessed, lastRun.ClaimsMatched,
            byBucket.ToDictionary(b => b.Bucket.ToString(), b => b.Count));
    }
}
