using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Vocabulary;
using QuizModel = VocabularyTrainer.Models.Quiz;

namespace VocabularyTrainer.Services.Quiz;

/// <summary>
/// Presenter for typing-mode quizzes where the user types a free-text answer.
/// Supports multi-option answers: any option (after bracket-stripping) is accepted as correct.
/// </summary>
public class TypingQuizPresenter : IQuizPresenter
{
    private readonly QuizModel _quiz;
    private readonly WordWeightStrategy _weightStrategy;
    private readonly WordListService _wordListService;
    private readonly LetterHintTracker? _hintTracker;
    private readonly IReadOnlyList<string> _options;
    private QuizResult _result = QuizResult.Pending;

    public TypingQuizPresenter(QuizModel quiz, WordWeightStrategy weightStrategy, WordListService wordListService, bool revealLetters)
    {
        _quiz = quiz;
        _weightStrategy = weightStrategy;
        _wordListService = wordListService;
        _hintTracker = revealLetters ? new LetterHintTracker() : null;
        _options = AnswerParser.Options(quiz.CorrectAnswer);
    }

    public void OnAnswerSelected(string typed)
    {
        if (_result == QuizResult.Correct || _result == QuizResult.MaxAttemptsReached)
            return;

        var normTyped = Normalize(typed);
        if (_options.Any(opt => normTyped == Normalize(opt)))
        {
            _result = QuizResult.Correct;
            _weightStrategy.RegisterCorrect(_quiz.WordEntry);
            _wordListService.SaveWords();
            return;
        }

        // Check for wrong Dutch article (correct noun but wrong article)
        if (_options.Any(opt => IsWrongArticle(typed, opt)))
        {
            _result = QuizResult.WrongArticle;
            _weightStrategy.RegisterMistake(_quiz.WordEntry);
            _wordListService.SaveWords();
            return;
        }

        _result = QuizResult.Wrong;
        _weightStrategy.RegisterMistake(_quiz.WordEntry);
        _wordListService.SaveWords();

        _hintTracker?.Update(typed, _options);
    }

    public QuizResult GetResult() => _result;

    public string GetCorrectAnswer() => AnswerParser.Canonical(_quiz.CorrectAnswer);

    public string? GetHint() => _hintTracker?.GetHint(_options);

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
