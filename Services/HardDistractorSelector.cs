using System;
using System.Collections.Generic;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Selects distractors that are visually similar to the correct word in the source language (Dutch).
/// Uses normalized Levenshtein distance (distance ÷ max-length) on the Question field.
/// Ranks candidates by normalized distance, takes top K = 2×count, then randomly picks
/// <paramref name="count"/> from those K to preserve variety.
/// Falls back to random selection when no candidate is close enough (normalized distance > 0.5).
/// </summary>
public class HardDistractorSelector : IDistractorSelector
{
    private const double SimilarityThreshold = 0.5;

    public IReadOnlyList<WordEntry> Select(IReadOnlyList<WordEntry> candidates, WordEntry correct, int count)
    {
        if (candidates.Count == 0)
            return [];

        var sorted = candidates
            .OrderBy(w => StringDistance.NormalizedLevenshtein(
                w.Question.ToLowerInvariant(), correct.Question.ToLowerInvariant()))
            .ToList();

        // If even the closest word is not visually similar, fall back to random
        double closestDistance = StringDistance.NormalizedLevenshtein(
            sorted[0].Question.ToLowerInvariant(), correct.Question.ToLowerInvariant());

        if (closestDistance > SimilarityThreshold)
            return candidates.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();

        int k = Math.Min(sorted.Count, 2 * count);
        return sorted.Take(k).OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }
}
