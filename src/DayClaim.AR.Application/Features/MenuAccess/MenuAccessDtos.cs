namespace DayClaim.AR.Application.Features.MenuAccess;

/// <summary>AllowAll is true only for Admin (who always sees every menu, with no
/// stored rows); everyone else's visible menus are exactly AllowedPaths.</summary>
public record MyMenuAccessDto(bool AllowAll, IReadOnlyCollection<string> AllowedPaths);
