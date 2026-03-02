using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Static utility for detecting grammatical word groups based on Dutch language patterns.
/// </summary>
public static class WordGrouping
{
    /// <summary>
    /// Detects the grammatical group of a Dutch word based on language-specific patterns.
    /// </summary>
    /// <param name="word">The Dutch word to analyze.</param>
    /// <returns>
    /// <see cref="WordGroup.Noun"/> if the word starts with "de" or "het" article,
    /// <see cref="WordGroup.Verb"/> else if the word ends with "-en" (infinitive form),
    /// otherwise <see cref="WordGroup.Other"/>.
    /// </returns>
    public static WordGroup Detect(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return WordGroup.Other;

        var normalized = word.Trim().ToLowerInvariant();

        // noun: starts with article
        if (normalized.StartsWith("de ") || normalized.StartsWith("het "))
            return WordGroup.Noun;

        // verb infinitive: ends with -en
        if (normalized.EndsWith("en"))
            return WordGroup.Verb;

        return WordGroup.Other;
    }
}