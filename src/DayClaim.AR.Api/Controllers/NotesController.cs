using DayClaim.AR.Application.Features.Notes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

public class NotesController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpGet("scenarios")]
    public async Task<ActionResult<IReadOnlyCollection<ScenarioDto>>> GetScenarios([FromQuery] Guid clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetScenariosQuery(clientOrganizationId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("scenarios")]
    public async Task<ActionResult<ScenarioDto>> CreateScenario(CreateScenarioCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("claim-notes")]
    public async Task<ActionResult<ClaimNoteDto>> AddClaimNote(AddClaimNoteCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
