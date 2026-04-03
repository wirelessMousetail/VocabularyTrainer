using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

/// <summary>
/// Presenter for typing-mode quizzes where the user types a free-text answer.
/// </summary>
public class TypingQuizPresenter : IQuizPresenter
{
    private readonly Quiz _quiz;
    private readonly WordWeightStrategy _weightStrategy;
    private readonly WordListService _wordListService;
    private readonly bool _revealLetters;
    private int _attemptCount;
    private QuizResult _result = QuizResult.Pending;

    public TypingQuizPresenter(Quiz quiz, WordWeightStrategy weightStrategy, WordListService wordListService, bool revealLetters)
    {
        _quiz = quiz;
        _weightStrategy = weightStrategy;
        _wordListService = wordListService;
        _revealLetters = revealLetters;
    }

    public void OnAnswerSelected(string typed)
    {
        if (_result == QuizResult.Correct || _result == QuizResult.MaxAttemptsReached)
            return;

        _attemptCount++;

        if (Normalize(typed) == Normalize(_quiz.CorrectAnswer))
        {
            _result = QuizResult.Correct;
            _weightStrategy.RegisterCorrect(_quiz.WordEntry);
            _wordListService.SaveWords();
            return;
        }

        // Check for wrong Dutch article (correct noun but wrong article)
        if (IsWrongArticle(typed, _quiz.CorrectAnswer))
        {
            _result = QuizResult.WrongArticle;
            _weightStrategy.RegisterMistake(_quiz.WordEntry);
            _wordListService.SaveWords();
            return;
        }

        _result = QuizResult.Wrong;
        _weightStrategy.RegisterMistake(_quiz.WordEntry);
        _wordListService.SaveWords();
    }

    public QuizResult GetResult() => _result;

    public string GetCorrectAnswer() => _quiz.CorrectAnswer;

    public string? GetHint()
    {
        if (!_revealLetters || _attemptCount == 0)
            return null;
        return BuildLetterRevealHint();
    }

    private string BuildLetterRevealHint()
    {
        var correct = _quiz.CorrectAnswer;
        var revealed = _attemptCount;
        var chars = new char[correct.Length];
        for (int i = 0; i < correct.Length; i++)
        {
            if (i < revealed || correct[i] == ' ')
                chars[i] = correct[i];
            else
                chars[i] = '_';
        }
        return new string(chars);
    }

    private static bool IsWrongArticle(string typed, string correct)
    {
        var normTyped = typed.Trim().ToLowerInvariant();
        var normCorrect = correct.Trim().ToLowerInvariant();

        if (!HasDutchArticle(normCorrect))
            return false;
        if (!HasDutchArticle(normTyped))
            return false;

        return StripDutchArticle(normTyped) == StripDutchArticle(normCorrect);
    }

    private static string Normalize(string s)
    {
        var t = s.Trim().ToLowerInvariant();
        if (t.StartsWith("the ")) t = t.Substring(4);
        else if (t.StartsWith("an ")) t = t.Substring(3);
        else if (t.StartsWith("a ")) t = t.Substring(2);
        return t;
    }

    private static bool HasDutchArticle(string normalized)
        => normalized.StartsWith("de ") || normalized.StartsWith("het ");

    private static string StripDutchArticle(string normalized)
    {
        if (normalized.StartsWith("de ")) return normalized.Substring(3);
        if (normalized.StartsWith("het ")) return normalized.Substring(4);
        return normalized;
    }
}
