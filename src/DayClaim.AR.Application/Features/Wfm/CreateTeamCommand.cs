using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using DayClaim.AR.Domain.Wfm;
using FluentValidation;
using MediatR;

namespace DayClaim.AR.Application.Features.Wfm;

public record CreateTeamCommand(Guid ClientOrganizationId, string Name, Guid TeamLeaderUserId, IReadOnlyCollection<Guid> MemberUserIds)
    : IRequest<TeamDto>;

public class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.ClientOrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TeamLeaderUserId).NotEmpty();
    }
}

public class CreateTeamCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = new Team
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ClientOrganizationId = request.ClientOrganizationId,
            Name = request.Name,
            TeamLeaderUserId = request.TeamLeaderUserId,
        };

        foreach (var userId in request.MemberUserIds.Distinct())
        {
            team.Members.Add(new TeamMember { TeamId = team.Id, Team = team, UserId = userId });
        }

        db.Teams.Add(team);
        await db.SaveChangesAsync(cancellationToken);

        return new TeamDto(team.Id, team.Name, team.TeamLeaderUserId, request.MemberUserIds.Distinct().ToArray());
    }
}
