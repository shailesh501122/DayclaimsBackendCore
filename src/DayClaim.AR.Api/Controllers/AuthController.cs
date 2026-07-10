using DayClaim.AR.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DayClaim.AR.Api.Controllers;

public record LoginRequest(string Username, string Password);
public record RefreshRequest(string RefreshToken);

[AllowAnonymous]
public class AuthController(ISender mediator) : ApiControllerBase(mediator)
{
    /// <summary>Central login shared by AR, EVBV and Coding (deck slide 22).</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await Mediator.Send(new LoginCommand(request.Username, request.Password, ip), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await Mediator.Send(new RefreshTokenCommand(request.RefreshToken, ip), cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContent();
    }
}
