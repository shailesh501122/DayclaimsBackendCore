using DayClaim.AR.Application.Common.Authorization;
using DayClaim.AR.Application.Common.Models;
using DayClaim.AR.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

/// <summary>Centralized user + RBAC management (deck slide 22) — Admin/Manager
/// only, with additional Admin-only escalation guards enforced in the
/// command handlers (see docs/SECURITY.md RBAC matrix).</summary>
[Authorize(Policy = PolicyNames.UserManagement)]
public class UsersController(ISender mediator) : ApiControllerBase(mediator)
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetUsersQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyCollection<RoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetRolesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetUsers), new { }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserRequestBody body, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new UpdateUserCommand(id, body.Email, body.DisplayName, body.RoleNames, body.ClientOrganizationIds),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<UserDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SetUserActiveCommand(id, true), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<UserDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SetUserActiveCommand(id, false), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}

public record UpdateUserRequestBody(
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> RoleNames,
    IReadOnlyCollection<Guid> ClientOrganizationIds);
