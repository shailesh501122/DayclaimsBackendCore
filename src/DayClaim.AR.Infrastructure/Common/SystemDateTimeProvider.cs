using DayClaim.AR.Application.Common.Interfaces;

namespace DayClaim.AR.Infrastructure.Common;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
