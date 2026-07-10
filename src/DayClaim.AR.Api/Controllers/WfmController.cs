using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Features.Wfm;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>WFM — team/allocation-rule CRUD is centralized, allocation execution runs per client (deck slide 12).</summary>
[Route("api/v{version:apiVersion}/wfm")]
[Authorize(Policy = PolicyNames.SupervisorOrAbove)]
public class WfmController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpPost("teams")]
    public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("allocation-rules")]
    public async Task<ActionResult<AllocationRuleDto>> CreateAllocationRule(CreateAllocationRuleCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Stage 3 of the AR workflow: Data Distribution (auto-allocation).</summary>
    [HttpPost("allocate")]
    public async Task<ActionResult<AllocationResultSummaryDto>> Allocate([FromBody] Guid clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RunAutoAllocationCommand(clientOrganizationId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("rollback")]
    public async Task<IActionResult> Rollback(RollbackAllocationCommand command, CancellationToken cancellationToken)
    {
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
