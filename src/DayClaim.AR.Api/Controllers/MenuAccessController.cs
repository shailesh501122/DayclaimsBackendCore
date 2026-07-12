using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Features.MenuAccess;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>Per-user frontend menu visibility (see docs/SECURITY.md). Unlike
/// UsersController, this is open to any authenticated user for the "me"
/// endpoint — every role needs to know its own visible menus — but managing
/// another user's menu grants is Admin-only.</summary>
[Authorize(Policy = PolicyNames.AnyAuthenticatedUser)]
public class MenuAccessController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpGet("me")]
    public async Task<ActionResult<MyMenuAccessDto>> GetMine(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyMenuAccessQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetForUser(Guid userId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserMenuAccessQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{userId:guid}")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<IActionResult> SetForUser(Guid userId, SetMenuAccessRequestBody body, CancellationToken cancellationToken)
    {
        await Mediator.Send(new SetUserMenuAccessCommand(userId, body.MenuPaths), cancellationToken);
        return NoContent();
    }
}

public record SetMenuAccessRequestBody(IReadOnlyCollection<string> MenuPaths);
