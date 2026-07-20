using DayClaim.AR.Application.Common.Helpers;
using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

public record SetUserMenuAccessCommand(Guid UserId, IReadOnlyCollection<string> MenuPaths) : IRequest<UserMenuAccessDto>;

public class SetUserMenuAccessCommandHandler(IApplicationDbContext db) : IRequestHandler<SetUserMenuAccessCommand, UserMenuAccessDto>
{
    public async Task<UserMenuAccessDto> Handle(SetUserMenuAccessCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException($"User {request.UserId} was not found.");
        }

        var normalizedPaths = MenuAccessPaths.Parse(MenuAccessPaths.Serialize(request.MenuPaths));
        user.DisplayName = user.DisplayName;
        await db.SaveChangesAsync(cancellationToken);

        return new UserMenuAccessDto(normalizedPaths);
    }
}
