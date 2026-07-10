using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Enums;
using FluentValidation;
using MediatR;

namespace DayClaim.AR.Application.Features.RuleEngine;

/// <summary>Rule CRUD is centralized (deck slide 12): one rule can target Global/Internal/Client/Payer scope.</summary>
public record CreateRuleCommand(
    string Name,
    RuleScope Scope,
    Guid? ClientOrganizationId,
    Guid? PayerMasterId,
    string ConditionExpression,
    ClaimBucket ResultBucket,
    ClaimPriority? ResultPriority,
    bool ExcludeSpecialProjectClaims,
    bool ExcludeManualAssignmentClaims,
    int EvaluationOrder) : IRequest<RuleDto>;

public class CreateRuleCommandValidator : AbstractValidator<CreateRuleCommand>
{
    public CreateRuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ConditionExpression).NotEmpty();
        RuleFor(x => x.ClientOrganizationId)
            .NotEmpty()
            .When(x => x.Scope is RuleScope.Client or RuleScope.Payer)
            .WithMessage("A client organization is required for client/payer-scoped rules.");
    }
}

public class CreateRuleCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateRuleCommand, RuleDto>
{
    public async Task<RuleDto> Handle(CreateRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = new Domain.RuleEngine.Rule
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Name = request.Name,
            Scope = request.Scope,
            ClientOrganizationId = request.ClientOrganizationId,
            PayerMasterId = request.PayerMasterId,
            ConditionExpression = request.ConditionExpression,
            ResultBucket = request.ResultBucket,
            ResultPriority = request.ResultPriority,
            ExcludeSpecialProjectClaims = request.ExcludeSpecialProjectClaims,
            ExcludeManualAssignmentClaims = request.ExcludeManualAssignmentClaims,
            EvaluationOrder = request.EvaluationOrder,
            IsActive = true,
        };

        db.Rules.Add(rule);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(rule);
    }

    internal static RuleDto ToDto(Domain.RuleEngine.Rule rule) => new(
        rule.Id, rule.Name, rule.Scope, rule.ClientOrganizationId, rule.PayerMasterId, rule.ConditionExpression,
        rule.ResultBucket, rule.ResultPriority, rule.ExcludeSpecialProjectClaims, rule.ExcludeManualAssignmentClaims,
        rule.EvaluationOrder, rule.IsActive);
}
