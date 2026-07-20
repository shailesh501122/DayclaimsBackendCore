using DayClaim.AR.Application.Common.Helpers;

namespace DayClaim.AR.UnitTests;

public class MenuAccessPathsTests
{
    [Fact]
    public void SerializeNormalizesAndDeduplicatesPaths()
    {
        var paths = new[] { " /dashboard ", "dashboard", "", "reports" };

        var serialized = MenuAccessPaths.Serialize(paths);

        Assert.Equal("[\"dashboard\",\"reports\"]", serialized);
    }

    [Fact]
    public void ParseReturnsEmptyCollectionForNullOrWhitespace()
    {
        Assert.Empty(MenuAccessPaths.Parse(null));
        Assert.Empty(MenuAccessPaths.Parse("   "));
    }
}
