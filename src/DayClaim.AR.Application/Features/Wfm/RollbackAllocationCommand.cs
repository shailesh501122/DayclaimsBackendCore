using DayClaim.AR.Application.Common.Exceptions;
using DayClaim.AR.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Wfm;

/// <summary>Roll back a claim's allocation (deck: "Roll Back") — a soft, audited state change, never a delete.</summary>
public record RollbackAllocationCommand(Guid AllocationId, string Reason) : IRequest;

public class RollbackAllocationCommandValidator : AbstractValidator<RollbackAllocationCommand>
{
    public RollbackAllocationCommandValidator()
    {
        RuleFor(x => x.AllocationId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class RollbackAllocationCommandHandler(IApplicationDbContext db, IDateTimeProvider clock) : IRequestHandler<RollbackAllocationCommand>
{
    public async Task Handle(RollbackAllocationCommand request, CancellationToken cancellationToken)
    {
        var allocation = await db.Allocations.FirstOrDefaultAsync(a => a.Id == request.AllocationId, cancellationToken)
            ?? throw new NotFoundException("Allocation", request.AllocationId);

        allocation.RolledBackAtUtc = clock.UtcNow;
        allocation.RollbackReason = request.Reason;

        await db.SaveChangesAsync(cancellationToken);
    }
}
