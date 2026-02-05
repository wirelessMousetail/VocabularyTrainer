using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public static class WordGrouping
{
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