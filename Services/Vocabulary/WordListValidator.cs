using System.Text.RegularExpressions;

namespace VocabularyTrainer.Services.Vocabulary;

public static class WordListValidator
{
    private static readonly Regex AllowedAnswerChars = new(@"^[a-zA-Z0-9 ,()'.\-]+$", RegexOptions.Compiled);

    public static void Validate(string question, string answer, int lineNumber)
    {
        if (!AllowedAnswerChars.IsMatch(answer))
            throw new FormatException($"Line {lineNumber}: answer contains invalid characters: '{answer}'");

        if (ContainsCommaOrBracket(question))
            throw new FormatException($"Line {lineNumber}: question contains comma or bracket: '{question}'");
    }

    private static bool ContainsCommaOrBracket(string s) =>
        s.Any(c => c == ',' || c == '(' || c == ')');
}
