using DayClaim.AR.Application.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>Backs the frontend's Inventory/Assignment/Rule-Engine summary & dashboard modules.</summary>
public class DashboardController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpGet("client-organizations")]
    public async Task<ActionResult<IReadOnlyCollection<ClientOrganizationDto>>> ClientOrganizations(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetClientOrganizationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("inventory-summary")]
    public async Task<ActionResult<InventorySummaryDto>> InventorySummary([FromQuery] Guid clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetInventorySummaryQuery(clientOrganizationId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("assignment-summary")]
    public async Task<ActionResult<AssignmentSummaryDto>> AssignmentSummary([FromQuery] Guid clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAssignmentSummaryQuery(clientOrganizationId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("rule-engine-summary")]
    public async Task<ActionResult<RuleEngineSummaryDto>> RuleEngineSummary([FromQuery] Guid clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetRuleEngineSummaryQuery(clientOrganizationId), cancellationToken);
        return Ok(result);
    }
}
