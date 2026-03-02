namespace VocabularyTrainer.Models;

/// <summary>
/// Root application settings container.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets the interval in seconds between automatic quiz popups.
    /// Default is 300 seconds (5 minutes).
    /// </summary>
    public int QuizIntervalSeconds { get; init; } = 300;

    /// <summary>
    /// Gets the quiz-specific configuration settings.
    /// </summary>
    public QuizConfiguration QuizConfiguration { get; init; } = new();
}
