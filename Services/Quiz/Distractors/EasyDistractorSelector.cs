using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services.Quiz.Distractors;

/// <summary>
/// Selects distractors randomly from the candidate pool.
/// </summary>
public class EasyDistractorSelector : IDistractorSelector
{
    public IReadOnlyList<WordEntry> Select(IReadOnlyList<WordEntry> candidates, WordEntry correct, int count)
    {
        return candidates
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .ToList();
    }
}
