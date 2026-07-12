using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.MenuAccess;

public record GetMyMenuAccessQuery : IRequest<MyMenuAccessDto>;

public class GetMyMenuAccessQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyMenuAccessQuery, MyMenuAccessDto>
{
    public async Task<MyMenuAccessDto> Handle(GetMyMenuAccessQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole("Admin"))
        {
            return new MyMenuAccessDto(true, Array.Empty<string>());
        }

        var paths = await db.UserMenuAccess
            .Where(m => m.UserId == currentUser.UserId)
            .Select(m => m.MenuPath)
            .ToArrayAsync(cancellationToken);

        return new MyMenuAccessDto(false, paths);
    }
}
