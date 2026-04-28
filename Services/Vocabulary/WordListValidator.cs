using System.Text.RegularExpressions;

namespace VocabularyTrainer.Services.Vocabulary;

public static class WordListValidator
{
    // Latin script only: ASCII letters/digits + accented Latin (U+00C0–U+00FF, excluding × and ÷)
    private static readonly Regex AllowedQuestionChars = new(
        @"^[a-zA-Z0-9À-ÖØ-öø-ÿ '\-]+$", RegexOptions.Compiled);

    private static readonly Regex AllowedAnswerChars = new(@"^[a-zA-Z0-9 ,'.\-!]+$", RegexOptions.Compiled);
    private static readonly Regex BracketGroup = new(@"\([^()]*\)", RegexOptions.Compiled);

    public static void Validate(string question, string answer, int lineNumber)
    {
        ValidateQuestion(question, lineNumber);
        ValidateAnswer(answer, lineNumber);
    }

    private static void ValidateQuestion(string question, int lineNumber)
    {
        if (!AllowedQuestionChars.IsMatch(question))
            throw new FormatException($"Line {lineNumber}: question contains invalid characters: '{question}'");
    }

    private static void ValidateAnswer(string answer, int lineNumber)
    {
        foreach (Match m in BracketGroup.Matches(answer))
        {
            var content = m.Value[1..^1];
            if (content.Contains(';'))
                throw new FormatException($"Line {lineNumber}: commentary contains ';': '{content}'");
        }

        var stripped = BracketGroup.Replace(answer, "");
        if (stripped.Contains('(') || stripped.Contains(')'))
            throw new FormatException($"Line {lineNumber}: answer contains unclosed or nested brackets: '{answer}'");

        var pureAnswer = stripped.Trim();
        if (pureAnswer.Length == 0)
            throw new FormatException($"Line {lineNumber}: answer has no content outside brackets: '{answer}'");

        if (!AllowedAnswerChars.IsMatch(pureAnswer))
            throw new FormatException($"Line {lineNumber}: answer contains invalid characters: '{pureAnswer}'");
    }
}
