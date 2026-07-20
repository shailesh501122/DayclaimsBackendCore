using System.Text.Json;

namespace DayClaim.AR.Application.Common.Helpers;

public static class MenuAccessPaths
{
    public static string Serialize(IEnumerable<string?>? paths)
    {
        var normalized = paths?
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!.Trim())
            .Where(path => path.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        return JsonSerializer.Serialize(normalized);
    }

    public static IReadOnlyCollection<string> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<string[]>(value);
            return parsed?
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => path!.Trim())
                .Where(path => path.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}
