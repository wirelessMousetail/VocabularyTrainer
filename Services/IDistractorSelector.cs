using System.Collections.Generic;
using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Selects distractor words for a quiz from a filtered candidate pool.
/// </summary>
public interface IDistractorSelector
{
    /// <summary>
    /// Selects <paramref name="count"/> distractor entries from <paramref name="candidates"/>.
    /// </summary>
    IReadOnlyList<WordEntry> Select(IReadOnlyList<WordEntry> candidates, WordEntry correct, int count);
}
