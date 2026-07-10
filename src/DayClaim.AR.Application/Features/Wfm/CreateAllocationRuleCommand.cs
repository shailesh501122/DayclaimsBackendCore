using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;
using FluentValidation;
using MediatR;

namespace DayClaim.AR.Application.Features.Wfm;

/// <summary>Deck: "Equal Distribution & Location based Allocation to User by Team Leader".</summary>
public record CreateAllocationRuleCommand(
    Guid ClientOrganizationId,
    Guid TeamId,
    ClaimBucket TargetBucket,
    ClaimPriority? TargetPriority,
    AllocationType AllocationType,
    bool EqualDistribution,
    bool LocationBased) : IRequest<AllocationRuleDto>;

public class CreateAllocationRuleCommandValidator : AbstractValidator<CreateAllocationRuleCommand>
{
    public CreateAllocationRuleCommandValidator()
    {
        RuleFor(x => x.ClientOrganizationId).NotEmpty();
        RuleFor(x => x.TeamId).NotEmpty();
    }
}

public class CreateAllocationRuleCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateAllocationRuleCommand, AllocationRuleDto>
{
    public async Task<AllocationRuleDto> Handle(CreateAllocationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = new Domain.Wfm.WfmAllocationRule
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ClientOrganizationId = request.ClientOrganizationId,
            TeamId = request.TeamId,
            TargetBucket = request.TargetBucket,
            TargetPriority = request.TargetPriority,
            AllocationType = request.AllocationType,
            EqualDistribution = request.EqualDistribution,
            LocationBased = request.LocationBased,
            IsActive = true,
        };

        db.WfmAllocationRules.Add(rule);
        await db.SaveChangesAsync(cancellationToken);

        return new AllocationRuleDto(rule.Id, rule.TeamId, rule.TargetBucket, rule.TargetPriority, rule.AllocationType, rule.EqualDistribution, rule.LocationBased, rule.IsActive);
    }
}
