using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services.Quiz;

public static class QuizDifficultyExtensions
{
    public static IDistractorSelector CreateSelector(this QuizDifficulty difficulty) => difficulty switch
    {
        QuizDifficulty.Hard => new HardDistractorSelector(),
        _ => new EasyDistractorSelector()
    };

    public static bool IsTypingMode(this QuizDifficulty d) => d == QuizDifficulty.Typing;
}
