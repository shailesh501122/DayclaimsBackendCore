using DayClaim.AR.Application.Common;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.ClaimEntries;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.RuleEngine;

/// <summary>
/// Rule Engine execution (deck slide 6, Stage 2): classifies every
/// not-yet-classified, approved claim for a client into a bucket/priority.
/// Special-project and manually-assigned claims are skipped when a matching
/// rule opts out of them, matching "Exclude Special Project & Manual Claims
/// Assignment" / "Exclude all standard Rules" from the workflow diagram.
/// </summary>
public record ExecuteRulesCommand(Guid ClientOrganizationId) : IRequest<RuleExecutionSummaryDto>;

public class ExecuteRulesCommandValidator : AbstractValidator<ExecuteRulesCommand>
{
    public ExecuteRulesCommandValidator() => RuleFor(x => x.ClientOrganizationId).NotEmpty();
}

public class ExecuteRulesCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    : IRequestHandler<ExecuteRulesCommand, RuleExecutionSummaryDto>
{
    public async Task<RuleExecutionSummaryDto> Handle(ExecuteRulesCommand request, CancellationToken cancellationToken)
    {
        var rules = await db.Rules
            .Where(r => r.IsActive && !r.IsDeleted &&
                        (r.ClientOrganizationId == null || r.ClientOrganizationId == request.ClientOrganizationId))
            .OrderBy(r => r.EvaluationOrder)
            .ToListAsync(cancellationToken);

        var entries = await db.RcmReportEntries
            .Where(e => e.ClientOrganizationId == request.ClientOrganizationId
                        && e.Bucket == null
                        && (e.Status == DataQualityStatus.AutoApproved || e.Status == DataQualityStatus.ManuallyApproved || e.Status == DataQualityStatus.ReOpened))
            .ToListAsync(cancellationToken);

        var run = new Domain.RuleEngine.RuleExecutionRun
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = clock.UtcNow,
            ClientOrganizationId = request.ClientOrganizationId,
            StartedAtUtc = clock.UtcNow,
            ClaimsProcessed = entries.Count,
            Status = "running",
        };
        db.RuleExecutionRuns.Add(run);

        var byBucket = new Dictionary<string, int>();
        var matched = 0;

        foreach (var entry in entries)
        {
            if (entry.Priority is ClaimPriority.SpecialProject && rules.Any(r => r.ExcludeSpecialProjectClaims))
            {
                continue;
            }
            if (entry.Priority is ClaimPriority.ManualAssignment && rules.Any(r => r.ExcludeManualAssignmentClaims))
            {
                continue;
            }

            var fields = RuleConditionEvaluator.MergeFields(
                entry.StandardFieldsJson, entry.CustomerSpecificFieldsJson, entry.CustomDefinedFieldsJson, entry.StatusStoringFieldsJson);

            var matchingRule = rules.FirstOrDefault(r => RuleConditionEvaluator.Evaluate(r.ConditionExpression, fields));
            if (matchingRule is null)
            {
                continue;
            }

            entry.Bucket = matchingRule.ResultBucket;
            entry.Priority = matchingRule.ResultPriority;
            matched++;

            var bucketKey = matchingRule.ResultBucket.ToString();
            byBucket[bucketKey] = byBucket.GetValueOrDefault(bucketKey) + 1;

            db.RuleExecutionResults.Add(new Domain.RuleEngine.RuleExecutionResult
            {
                Id = IdGenerator.NewId(),
                CreatedAtUtc = clock.UtcNow,
                RuleExecutionRunId = run.Id,
                RcmReportEntryId = entry.Id,
                RuleId = matchingRule.Id,
                Bucket = matchingRule.ResultBucket,
                Priority = matchingRule.ResultPriority,
            });
        }

        run.ClaimsMatched = matched;
        run.CompletedAtUtc = clock.UtcNow;
        run.Status = "completed";

        await db.SaveChangesAsync(cancellationToken);

        return new RuleExecutionSummaryDto(run.Id, run.ClientOrganizationId, run.ClaimsProcessed, run.ClaimsMatched, run.Status, byBucket);
    }
}
