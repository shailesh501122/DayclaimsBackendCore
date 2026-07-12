using DayClaim.AR.Domain.Identity;
using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Users;

/// <summary>Soft delete only (see AuditableEntity.IsDeleted) — nothing in
/// this domain is hard-deleted, for audit/history requirements. Admin-only:
/// enforced by controller policy, not repeated here, since removing a user
/// entirely is a stronger action than the Manager-level user edits above.</summary>
public record DeleteUserCommand(Guid Id) : IRequest;

public class DeleteUserCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        if (user.Id == currentUser.UserId)
        {
            throw new ForbiddenAccessException("You cannot delete your own account.");
        }

        user.IsDeleted = true;
        user.DeletedAtUtc = DateTimeOffset.UtcNow;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        user.UpdatedByUserId = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
    }
}
