using System.Globalization;
using System.Text.Json;

namespace DayClaim.AR.Application.Common;

/// <summary>
/// Deliberately tiny, safe expression evaluator for Rule Engine conditions —
/// NOT a general-purpose scripting engine (no eval/reflection/IO), so a rule
/// authored by a platform admin can never execute arbitrary code. Grammar:
/// <c>field op value (AND field op value)*</c> where op is one of
/// <c>== != &gt; &gt;= &lt; &lt;=</c>. Field values are read from the claim's
/// merged standard/customer-specific/custom-defined/status-storing JSON.
/// </summary>
public static class RuleConditionEvaluator
{
    public static bool Evaluate(string expression, IReadOnlyDictionary<string, string> fields)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return true;
        }

        var clauses = expression.Split("AND", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return clauses.All(clause => EvaluateClause(clause, fields));
    }

    private static bool EvaluateClause(string clause, IReadOnlyDictionary<string, string> fields)
    {
        foreach (var op in new[] { ">=", "<=", "==", "!=", ">", "<" })
        {
            var idx = clause.IndexOf(op, StringComparison.Ordinal);
            if (idx <= 0)
            {
                continue;
            }

            var field = clause[..idx].Trim();
            var rawValue = clause[(idx + op.Length)..].Trim().Trim('\'', '"');

            if (!fields.TryGetValue(field, out var actual))
            {
                return false;
            }

            return Compare(actual, op, rawValue);
        }

        return false;
    }

    private static bool Compare(string actual, string op, string expected)
    {
        if (decimal.TryParse(actual, NumberStyles.Any, CultureInfo.InvariantCulture, out var actualNum) &&
            decimal.TryParse(expected, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedNum))
        {
            return op switch
            {
                ">" => actualNum > expectedNum,
                ">=" => actualNum >= expectedNum,
                "<" => actualNum < expectedNum,
                "<=" => actualNum <= expectedNum,
                "==" => actualNum == expectedNum,
                "!=" => actualNum != expectedNum,
                _ => false,
            };
        }

        return op switch
        {
            "==" => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    /// <summary>Flattens the entry's four JSON sections into one lookup for evaluation.</summary>
    public static Dictionary<string, string> MergeFields(params string[] jsonSections)
    {
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var json in jsonSections)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                merged[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString() ?? string.Empty
                    : prop.Value.GetRawText();
            }
        }
        return merged;
    }
}
