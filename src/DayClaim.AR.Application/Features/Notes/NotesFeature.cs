using DayClaim.AR.Application.Common.Interfaces;
using DayClaim.AR.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DayClaim.AR.Application.Features.Notes;

public record ScenarioDto(Guid Id, string Name, string StatusActionSubActionMapping, string NoteTemplate, int DisplayOrder, bool IsActive);
public record ClaimNoteDto(Guid Id, Guid RcmReportEntryId, Guid UserId, string NoteText, string? ActionTaken, string? StatusSet, DateTimeOffset CreatedAtUtc);

public record CreateScenarioCommand(Guid ClientOrganizationId, string Name, string StatusActionSubActionMapping, string NoteTemplate, int DisplayOrder)
    : IRequest<ScenarioDto>;

public class CreateScenarioCommandValidator : AbstractValidator<CreateScenarioCommand>
{
    public CreateScenarioCommandValidator()
    {
        RuleFor(x => x.ClientOrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NoteTemplate).NotEmpty();
    }
}

public class CreateScenarioCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateScenarioCommand, ScenarioDto>
{
    public async Task<ScenarioDto> Handle(CreateScenarioCommand request, CancellationToken cancellationToken)
    {
        var scenario = new Domain.Notes.ScenarioMaster
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ClientOrganizationId = request.ClientOrganizationId,
            Name = request.Name,
            StatusActionSubActionMapping = request.StatusActionSubActionMapping,
            NoteTemplate = request.NoteTemplate,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
        };
        db.ScenarioMasters.Add(scenario);
        await db.SaveChangesAsync(cancellationToken);
        return new ScenarioDto(scenario.Id, scenario.Name, scenario.StatusActionSubActionMapping, scenario.NoteTemplate, scenario.DisplayOrder, scenario.IsActive);
    }
}

public record AddClaimNoteCommand(Guid RcmReportEntryId, Guid UserId, Guid? ScenarioMasterId, string NoteText, string? ActionTaken, string? StatusSet)
    : IRequest<ClaimNoteDto>;

public class AddClaimNoteCommandValidator : AbstractValidator<AddClaimNoteCommand>
{
    public AddClaimNoteCommandValidator()
    {
        RuleFor(x => x.RcmReportEntryId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NoteText).NotEmpty().MaximumLength(4000);
    }
}

public class AddClaimNoteCommandHandler(IApplicationDbContext db) : IRequestHandler<AddClaimNoteCommand, ClaimNoteDto>
{
    public async Task<ClaimNoteDto> Handle(AddClaimNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new Domain.Notes.ClaimNote
        {
            Id = IdGenerator.NewId(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            RcmReportEntryId = request.RcmReportEntryId,
            ScenarioMasterId = request.ScenarioMasterId,
            UserId = request.UserId,
            NoteText = request.NoteText,
            ActionTaken = request.ActionTaken,
            StatusSet = request.StatusSet,
        };
        db.ClaimNotes.Add(note);
        await db.SaveChangesAsync(cancellationToken);
        return new ClaimNoteDto(note.Id, note.RcmReportEntryId, note.UserId, note.NoteText, note.ActionTaken, note.StatusSet, note.CreatedAtUtc);
    }
}

public record GetScenariosQuery(Guid ClientOrganizationId) : IRequest<IReadOnlyCollection<ScenarioDto>>;

public class GetScenariosQueryHandler(IApplicationDbContext db) : IRequestHandler<GetScenariosQuery, IReadOnlyCollection<ScenarioDto>>
{
    public async Task<IReadOnlyCollection<ScenarioDto>> Handle(GetScenariosQuery request, CancellationToken cancellationToken)
    {
        var scenarios = await db.ScenarioMasters
            .Where(s => s.ClientOrganizationId == request.ClientOrganizationId && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

        return scenarios.Select(s => new ScenarioDto(s.Id, s.Name, s.StatusActionSubActionMapping, s.NoteTemplate, s.DisplayOrder, s.IsActive)).ToArray();
    }
}
