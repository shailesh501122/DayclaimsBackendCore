using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Wfm;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Wfm;

/// <summary>
/// Data Distribution (deck slide 6, Stage 3): automatically assigns
/// classified-but-unassigned claims to team members using equal-distribution
/// round robin, honoring each allocation rule's target bucket/priority.
/// </summary>
public record RunAutoAllocationCommand(Guid ClientOrganizationId) : IRequest<AllocationResultSummaryDto>;

public class RunAutoAllocationCommandValidator : AbstractValidator<RunAutoAllocationCommand>
{
    public RunAutoAllocationCommandValidator() => RuleFor(x => x.ClientOrganizationId).NotEmpty();
}

public class RunAutoAllocationCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IRequestHandler<RunAutoAllocationCommand, AllocationResultSummaryDto>
{
    public async Task<AllocationResultSummaryDto> Handle(RunAutoAllocationCommand request, CancellationToken cancellationToken)
    {
        var rules = await db.WfmAllocationRules
            .Where(r => r.ClientOrganizationId == request.ClientOrganizationId && r.IsActive && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var alreadyAllocatedEntryIds = await db.Allocations
            .Where(a => a.RolledBackAtUtc == null)
            .Select(a => a.RcmReportEntryId)
            .ToListAsync(cancellationToken);

        var byUser = new Dictionary<string, int>();
        var totalAllocated = 0;

        foreach (var rule in rules)
        {
            var members = await db.TeamMembers.Where(m => m.TeamId == rule.TeamId).Select(m => m.UserId).ToListAsync(cancellationToken);
            if (members.Count == 0)
            {
                continue;
            }

            var candidateEntries = await db.RcmReportEntries
                .Where(e => e.ClientOrganizationId == request.ClientOrganizationId
                            && e.Bucket == rule.TargetBucket
                            && (rule.TargetPriority == null || e.Priority == rule.TargetPriority)
                            && !alreadyAllocatedEntryIds.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            for (var i = 0; i < candidateEntries.Count; i++)
            {
                // Equal-distribution round robin across the team (deck: "Equal Distribution... to User by Team Leader").
                var assignee = members[i % members.Count];

                db.Allocations.Add(new Allocation
                {
                    Id = IdGenerator.NewId(),
                    CreatedAtUtc = clock.UtcNow,
                    RcmReportEntryId = candidateEntries[i],
                    AssignedToUserId = assignee,
                    AllocationType = rule.AllocationType,
                    AllocatedAtUtc = clock.UtcNow,
                });

                var key = assignee.ToString();
                byUser[key] = byUser.GetValueOrDefault(key) + 1;
                totalAllocated++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new AllocationResultSummaryDto(totalAllocated, byUser);
    }
}
