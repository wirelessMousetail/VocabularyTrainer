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
    private bool[]? _revealMask;
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

        if (_revealLetters)
            UpdateReveal(typed);
    }

    public QuizResult GetResult() => _result;

    public string GetCorrectAnswer() => _quiz.CorrectAnswer;

    public string? GetHint()
    {
        if (!_revealLetters || _revealMask == null)
            return null;
        return BuildLetterRevealHint();
    }

    private void UpdateReveal(string typed)
    {
        var lowerTyped = typed.Trim().ToLowerInvariant();
        var lowerCorrect = _quiz.CorrectAnswer.Trim().ToLowerInvariant();

        var newMask = ComputeRevealMask(lowerTyped, lowerCorrect, minBlock: 3);

        if (newMask != null)
        {
            // Gate opened — merge with existing mask (revealed letters are never hidden again)
            if (_revealMask == null)
                _revealMask = newMask;
            else
                for (int i = 0; i < _revealMask.Length; i++)
                    _revealMask[i] |= newMask[i];
        }
        else if (Random.Shared.Next(3) == 0)
        {
            // Gate still closed — random bonus: reveal the leftmost not-yet-revealed non-space char
            _revealMask ??= new bool[lowerCorrect.Length];
            for (int i = 0; i < lowerCorrect.Length; i++)
            {
                if (!_revealMask[i] && lowerCorrect[i] != ' ')
                {
                    _revealMask[i] = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Aligns <paramref name="typed"/> against <paramref name="correct"/> using edit distance,
    /// then returns a boolean mask of matched positions in <paramref name="correct"/>.
    /// Returns null when no contiguous run of matches reaches <paramref name="minBlock"/> chars
    /// (gate stays closed).
    /// Once the gate opens, all matched positions are revealed — including isolated single chars —
    /// so that every genuinely correct letter is shown.
    /// </summary>
    private static bool[]? ComputeRevealMask(string typed, string correct, int minBlock)
    {
        int m = typed.Length;
        int n = correct.Length;

        // Build DP table for edit distance
        var dp = new int[m + 1, n + 1];
        for (int i = 0; i <= m; i++) dp[i, 0] = i;
        for (int j = 0; j <= n; j++) dp[0, j] = j;

        for (int i = 1; i <= m; i++)
        for (int j = 1; j <= n; j++)
        {
            dp[i, j] = typed[i - 1] == correct[j - 1]
                ? dp[i - 1, j - 1]
                : 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
        }

        // Backtrace to find which positions in correct are matched.
        // Tie-breaking rule: when a diagonal match ties with skipping a correct char,
        // prefer the skip — this aligns typed chars with earlier (leftmost) correct chars,
        // giving more intuitive hints (e.g. "bezet_en" instead of "beze_ten" for bezeten/bezetten).
        var matched = new bool[n];
        int ci = m, cj = n;
        while (ci > 0 && cj > 0)
        {
            if (typed[ci - 1] == correct[cj - 1] && dp[ci, cj] == dp[ci - 1, cj - 1])
            {
                // Match is possible here. Check for skip-correct tie:
                // if there is at least one more correct char to the left AND skipping this
                // correct char costs the same, do the skip so the typed char aligns earlier.
                if (cj > 1 && dp[ci, cj - 1] + 1 == dp[ci, cj])
                    cj--;
                else
                {
                    matched[cj - 1] = true;
                    ci--; cj--;
                }
            }
            else if (dp[ci, cj] == dp[ci - 1, cj - 1] + 1)
            {
                // Substitution
                ci--; cj--;
            }
            else if (dp[ci, cj] == dp[ci - 1, cj] + 1)
            {
                // Delete from typed (advance typed only)
                ci--;
            }
            else
            {
                // Skip correct char (advance correct only)
                cj--;
            }
        }

        // Gate check: only open if at least one contiguous block of matches is ≥ minBlock
        int blockLen = 0;
        bool gateOpen = false;
        for (int j = 0; j < n; j++)
        {
            blockLen = matched[j] ? blockLen + 1 : 0;
            if (blockLen >= minBlock) { gateOpen = true; break; }
        }

        return gateOpen ? matched : null;
    }

    private string BuildLetterRevealHint()
    {
        var correct = _quiz.CorrectAnswer;
        var chars = new char[correct.Length];
        for (int i = 0; i < correct.Length; i++)
        {
            if (_revealMask![i] || correct[i] == ' ')
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
