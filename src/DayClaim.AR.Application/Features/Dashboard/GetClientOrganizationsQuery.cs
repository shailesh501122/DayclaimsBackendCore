using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Dashboard;

public record ClientOrganizationDto(Guid Id, string Name, string Code);

/// <summary>Lets the frontend populate a client-org selector for the summary
/// endpoints below, which are all scoped to a single client organization.</summary>
public record GetClientOrganizationsQuery : IRequest<IReadOnlyCollection<ClientOrganizationDto>>;

public class GetClientOrganizationsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetClientOrganizationsQuery, IReadOnlyCollection<ClientOrganizationDto>>
{
    public async Task<IReadOnlyCollection<ClientOrganizationDto>> Handle(GetClientOrganizationsQuery request, CancellationToken cancellationToken) =>
        await db.ClientOrganizations
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new ClientOrganizationDto(c.Id, c.Name, c.Code))
            .ToArrayAsync(cancellationToken);
}
