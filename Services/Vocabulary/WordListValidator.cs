namespace VocabularyTrainer.Services.Vocabulary;

public static class WordListValidator
{
    public static void Validate(string answer, int lineNumber)
    {
        if (ContainsCyrillic(answer))
            throw new FormatException($"Line {lineNumber}: answer contains Cyrillic characters: '{answer}'");

        if (answer.Contains('/'))
            throw new FormatException($"Line {lineNumber}: answer contains '/' separator: '{answer}'");
    }

    private static bool ContainsCyrillic(string s) =>
        s.Any(c => c >= 'Ѐ' && c <= 'ӿ');
}
