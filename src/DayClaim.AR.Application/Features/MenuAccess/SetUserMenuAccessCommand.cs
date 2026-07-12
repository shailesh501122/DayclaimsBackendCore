using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.MenuAccess;

/// <summary>Replaces a user's entire allowed-menu set. Admin-only (enforced by
/// controller policy) — granting/revoking screen access is treated as
/// sensitive as granting a role, not delegated to Manager.</summary>
public record SetUserMenuAccessCommand(Guid UserId, IReadOnlyCollection<string> MenuPaths) : IRequest;

public class SetUserMenuAccessCommandValidator : AbstractValidator<SetUserMenuAccessCommand>
{
    public SetUserMenuAccessCommandValidator()
    {
        RuleForEach(x => x.MenuPaths).NotEmpty().MaximumLength(300);
    }
}

public class SetUserMenuAccessCommandHandler(IApplicationDbContext db) : IRequestHandler<SetUserMenuAccessCommand>
{
    public async Task Handle(SetUserMenuAccessCommand request, CancellationToken cancellationToken)
    {
        var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var existing = await db.UserMenuAccess.Where(m => m.UserId == request.UserId).ToListAsync(cancellationToken);
        foreach (var row in existing)
        {
            db.UserMenuAccess.Remove(row);
        }

        foreach (var path in request.MenuPaths.Distinct())
        {
            db.UserMenuAccess.Add(new UserMenuAccess { UserId = request.UserId, MenuPath = path });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
