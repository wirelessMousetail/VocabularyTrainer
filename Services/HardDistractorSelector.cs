using System;
using System.Collections.Generic;
using System.Linq;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Selects distractors that are visually similar to the correct word in the source language (Dutch).
/// Ranks candidates by Levenshtein distance on the Question field, takes the top K = 2×count,
/// then randomly picks <paramref name="count"/> from those K to preserve variety.
/// </summary>
public class HardDistractorSelector : IDistractorSelector
{
    public IReadOnlyList<WordEntry> Select(IReadOnlyList<WordEntry> candidates, WordEntry correct, int count)
    {
        int k = Math.Min(candidates.Count, 2 * count);
        return candidates
            .OrderBy(w => StringDistance.Levenshtein(
                w.Question.ToLowerInvariant(), correct.Question.ToLowerInvariant()))
            .Take(k)
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .ToList();
    }
}
