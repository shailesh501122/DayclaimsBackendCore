using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Features.RuleEngine;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>Rule Engine — CRUD is centralized, execution runs per client (deck slide 12).</summary>
[Route("api/v{version:apiVersion}/rule-engine")]
public class RuleEngineController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpGet("rules")]
    public async Task<ActionResult<IReadOnlyCollection<RuleDto>>> GetRules([FromQuery] Guid? clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetRulesQuery(clientOrganizationId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("rules")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<RuleDto>> CreateRule(CreateRuleCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRules), new { }, result);
    }

    /// <summary>Stage 2 of the AR workflow: classify claims into buckets/priorities.</summary>
    [HttpPost("execute")]
    [Authorize(Policy = PolicyNames.SupervisorOrAbove)]
    public async Task<ActionResult<RuleExecutionSummaryDto>> Execute([FromBody] Guid clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ExecuteRulesCommand(clientOrganizationId), cancellationToken);
        return Ok(result);
    }
}
