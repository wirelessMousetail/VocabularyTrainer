namespace VocabularyTrainer.Models;

public class AppSettings
{
    public int QuizIntervalSeconds { get; init; } = 300;
    public QuizConfiguration QuizConfiguration { get; init; } = new();
}
