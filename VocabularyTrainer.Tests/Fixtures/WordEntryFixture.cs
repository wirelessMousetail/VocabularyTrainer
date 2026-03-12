using VocabularyTrainer.Models;

namespace VocabularyTrainer.Tests.Fixtures;

internal static class WordEntryFixture
{
    public static WordEntry Make(
        string question,
        string answer,
        WordGroup? group = null,
        int weight = 0,
        int streak = 0) =>
        new(question, answer, new WeightData(weight, streak), group);
}
