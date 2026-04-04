using System.Text.RegularExpressions;

namespace VocabularyTrainer.Services;

/// <summary>
/// Pure static helpers for pre-processing answer strings before evaluation and hint display.
/// Processing order: strip parenthetical groups first, then split on commas.
/// </summary>
public static class AnswerParser
{
    private static readonly Regex BracketsPattern = new(@"\s*\([^)]*\)\s*", RegexOptions.Compiled);

    /// <summary>
    /// Strips all (...) groups (and surrounding whitespace) from <paramref name="raw"/>,
    /// returns trimmed result.
    /// </summary>
    public static string Canonical(string raw)
    {
        var result = BracketsPattern.Replace(raw, " ");
        return CollapseWhitespace(result).Trim();
    }

    /// <summary>
    /// Strips brackets first, then splits on ',', trims each part, removes empty entries.
    /// Returns at least one element (the canonical form if no comma).
    /// </summary>
    public static IReadOnlyList<string> Options(string raw)
    {
        var canonical = Canonical(raw);
        var parts = canonical
            .Split(',')
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        return parts.Count > 0 ? parts : [canonical];
    }

    private static string CollapseWhitespace(string s)
    {
        // Collapse multiple spaces into one (bracket removal can leave double-spaces)
        return Regex.Replace(s, @"  +", " ");
    }
}
