using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.MenuAccess;

/// <summary>For the Admin "assign menus" UI to preload a user's current selection.</summary>
public record GetUserMenuAccessQuery(Guid UserId) : IRequest<IReadOnlyCollection<string>>;

public class GetUserMenuAccessQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetUserMenuAccessQuery, IReadOnlyCollection<string>>
{
    public async Task<IReadOnlyCollection<string>> Handle(GetUserMenuAccessQuery request, CancellationToken cancellationToken) =>
        await db.UserMenuAccess
            .Where(m => m.UserId == request.UserId)
            .Select(m => m.MenuPath)
            .ToArrayAsync(cancellationToken);
}
