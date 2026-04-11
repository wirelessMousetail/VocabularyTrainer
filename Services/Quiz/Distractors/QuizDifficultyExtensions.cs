using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services.Quiz.Distractors;

/// <summary>
/// Extension methods for <see cref="QuizDifficulty"/> that map enum values to
/// concrete <see cref="IDistractorSelector"/> instances and quiz-mode checks.
/// </summary>
public static class QuizDifficultyExtensions
{
    /// <summary>
    /// Creates the appropriate <see cref="IDistractorSelector"/> for the given difficulty.
    /// Returns <see cref="HardDistractorSelector"/> for <see cref="QuizDifficulty.Hard"/>;
    /// returns <see cref="EasyDistractorSelector"/> for all other values.
    /// </summary>
    public static IDistractorSelector CreateSelector(this QuizDifficulty difficulty) => difficulty switch
    {
        QuizDifficulty.Hard => new HardDistractorSelector(),
        _ => new EasyDistractorSelector()
    };

    /// <summary>
    /// Returns true when the difficulty is <see cref="QuizDifficulty.Typing"/>,
    /// indicating a free-text quiz should be created instead of multiple-choice.
    /// </summary>
    public static bool IsTypingMode(this QuizDifficulty d) => d == QuizDifficulty.Typing;
}
