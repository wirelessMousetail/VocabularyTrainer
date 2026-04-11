namespace VocabularyTrainer.Models;

/// <summary>
/// Specifies the quiz format and distractor selection strategy.
/// </summary>
public enum QuizDifficulty
{
    /// <summary>
    /// Multiple-choice quiz with randomly selected distractors.
    /// </summary>
    Easy,

    /// <summary>
    /// Multiple-choice quiz where distractors are visually similar to the correct answer,
    /// making it harder to distinguish the right option.
    /// </summary>
    Hard,

    /// <summary>
    /// Free-text input quiz: the user types the answer instead of selecting from options.
    /// This changes the quiz format entirely — no multiple-choice options are generated.
    /// </summary>
    Typing
}
