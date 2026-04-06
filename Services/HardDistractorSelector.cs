using System;
using System.Collections.Generic;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Selects distractors that are visually similar to the correct word in the source language (Dutch).
/// Uses Jaro-Winkler similarity on the Question field, which gives a prefix bonus so words sharing
/// the same first letters score higher — matching human perception of visual similarity.
/// Ranks candidates by similarity descending, takes top K = 2×count, then randomly picks
/// <paramref name="count"/> from those K to preserve variety.
/// Falls back to random selection when no candidate is similar enough (Jaro-Winkler &lt; 0.7).
/// </summary>
public class HardDistractorSelector : IDistractorSelector
{
    private const double SimilarityThreshold = 0.7;

    public IReadOnlyList<WordEntry> Select(IReadOnlyList<WordEntry> candidates, WordEntry correct, int count)
    {
        if (candidates.Count == 0)
            return [];

        var sorted = candidates
            .OrderByDescending(w => StringDistance.JaroWinkler(
                w.Question.ToLowerInvariant(), correct.Question.ToLowerInvariant()))
            .ToList();

        double bestSimilarity = StringDistance.JaroWinkler(
            sorted[0].Question.ToLowerInvariant(), correct.Question.ToLowerInvariant());

        Console.WriteLine($"[HardSelector] correct='{correct.Question}' closest='{sorted[0].Question}' jw={bestSimilarity:F3}");

        if (bestSimilarity < SimilarityThreshold)
        {
            Console.WriteLine($"[HardSelector] no similar words found (threshold={SimilarityThreshold}) — falling back to random");
            return candidates.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
        }

        int k = Math.Min(sorted.Count, 2 * count);
        var selected = sorted.Take(k).OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
        Console.WriteLine($"[HardSelector] top-{k} pool: [{string.Join(", ", sorted.Take(k).Select(w => $"{w.Question}({StringDistance.JaroWinkler(w.Question.ToLowerInvariant(), correct.Question.ToLowerInvariant()):F3})"))}] → picked: [{string.Join(", ", selected.Select(w => w.Question))}]");
        return selected;
    }
}
