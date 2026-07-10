using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.RuleEngine;

/// <summary>Returns rules applicable to a client: Global + internal-wide + that client's own Client/Payer rules.</summary>
public record GetRulesQuery(Guid? ClientOrganizationId) : IRequest<IReadOnlyCollection<RuleDto>>;

public class GetRulesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetRulesQuery, IReadOnlyCollection<RuleDto>>
{
    public async Task<IReadOnlyCollection<RuleDto>> Handle(GetRulesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Rules.Where(r => !r.IsDeleted);

        if (request.ClientOrganizationId is { } clientId)
        {
            query = query.Where(r => r.ClientOrganizationId == null || r.ClientOrganizationId == clientId);
        }

        var rules = await query.OrderBy(r => r.EvaluationOrder).ToListAsync(cancellationToken);
        return rules.Select(CreateRuleCommandHandler.ToDto).ToArray();
    }
}
