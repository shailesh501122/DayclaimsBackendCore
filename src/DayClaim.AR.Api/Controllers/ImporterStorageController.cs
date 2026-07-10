using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Features.ImporterStorage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>
/// Per-client Importer Storage ingestion endpoint — the synchronous stand-in
/// for the Glue/stream ETL pipeline (deck slides 25-27). See
/// IngestFileCommand for the full mapping to the target design.
/// </summary>
[Route("api/v{version:apiVersion}/importer-storage")]
[Authorize(Policy = PolicyNames.SupervisorOrAbove)]
public class ImporterStorageController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpPost("ingest")]
    public async Task<ActionResult<IngestResultDto>> Ingest(IngestFileCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
