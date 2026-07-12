using System.Security.Claims;
using DayClaim.AR.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DayClaim.AR.Infrastructure.Security;

/// <summary>Reads the authenticated identity from the current HTTP request's validated JWT claims.</summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private static readonly HashSet<string> InternalStaffRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "Manager", "Supervisor", "User",
    };

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();

    public IReadOnlyCollection<Guid> ClientOrganizationIds =>
        Principal?.FindAll("org").Select(c => Guid.TryParse(c.Value, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue).Select(id => id!.Value).ToArray() ?? Array.Empty<Guid>();

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public bool IsInternalStaff => Roles.Any(InternalStaffRoles.Contains);
}
