namespace VocabularyTrainer.Models;

public class QuizConfiguration
{
    public int OptionCount { get; init; } = 3;
    public int AutoCloseAfterCorrectSeconds { get; init; } = 5;
    public bool ShowCorrectAnswerOnWrong { get; init; } = false;
    public int? MaxAttemptsPerQuiz { get; init; } = null;
    public QuizDirection Direction { get; init; } = QuizDirection.Direct;
}