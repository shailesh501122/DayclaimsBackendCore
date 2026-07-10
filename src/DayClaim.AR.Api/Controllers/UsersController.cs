using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Common.Models;
using DayClaim.AR.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>Centralized user + RBAC management (deck slide 22) — Super Admin / Site Admin only.</summary>
[Authorize(Policy = PolicyNames.AdminOnly)]
public class UsersController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetUsersQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetUsers), new { }, result);
    }
}
