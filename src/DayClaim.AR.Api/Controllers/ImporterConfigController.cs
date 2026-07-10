using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Features.ImporterConfig;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>Data Importer configuration — centralized, per-client field mapping (deck slide 23).</summary>
[Route("api/v{version:apiVersion}/importer-configs")]
public class ImporterConfigController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ImporterConfigDto>>> Get([FromQuery] Guid clientOrganizationId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetImporterConfigsQuery(clientOrganizationId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.SupervisorOrAbove)]
    public async Task<ActionResult<ImporterConfigDto>> Create(CreateImporterConfigCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { clientOrganizationId = result.ClientOrganizationId }, result);
    }
}
