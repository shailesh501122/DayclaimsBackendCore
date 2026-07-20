namespace DayClaim.AR.Application.Features.Users;

public record UserMenuAccessDto(IReadOnlyCollection<string> MenuPaths);

public record UserMenuAccessRequestDto(IReadOnlyCollection<string> MenuPaths);
