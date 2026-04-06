namespace VocabularyTrainer.Models;

/// <summary>
/// Configuration settings for quiz behavior and presentation.
/// </summary>
public class QuizConfiguration
{
    /// <summary>
    /// Gets the number of multiple-choice answer options to display (2-10).
    /// Default is 3.
    /// </summary>
    public int OptionCount { get; init; } = 3;

    /// <summary>
    /// Gets the number of seconds to wait before automatically closing the quiz window after a correct answer.
    /// Default is 5 seconds.
    /// </summary>
    public int AutoCloseAfterCorrectSeconds { get; init; } = 5;

    /// <summary>
    /// Gets a value indicating whether to show the correct answer when the user selects a wrong answer.
    /// Default is false.
    /// </summary>
    public bool ShowCorrectAnswerOnWrong { get; init; } = false;

    /// <summary>
    /// Gets the maximum number of answer attempts allowed per quiz, or null for unlimited attempts.
    /// Default is null (unlimited).
    /// </summary>
    public int? MaxAttemptsPerQuiz { get; init; } = null;

    /// <summary>
    /// Gets the quiz direction mode (Direct, Reverse, or Random).
    /// Default is <see cref="QuizDirection.Direct"/>.
    /// </summary>
    public QuizDirection Direction { get; init; } = QuizDirection.Direct;

    /// <summary>
    /// Gets the quiz difficulty mode (Easy uses random distractors, Hard uses similar-looking distractors).
    /// Default is <see cref="QuizDifficulty.Easy"/>.
    /// </summary>
    public QuizDifficulty Difficulty { get; init; } = QuizDifficulty.Easy;
}