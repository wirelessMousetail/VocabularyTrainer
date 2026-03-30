using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public static class QuizDifficultyExtensions
{
    public static IDistractorSelector CreateSelector(this QuizDifficulty difficulty) => difficulty switch
    {
        QuizDifficulty.Hard => new HardDistractorSelector(),
        _ => new EasyDistractorSelector()
    };
}
