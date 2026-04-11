using VocabularyTrainer.Models;
using VocabularyTrainer.Services.Vocabulary;
using QuizModel = VocabularyTrainer.Models.Quiz;

namespace VocabularyTrainer.Services.Quiz.Presenters;

/// <summary>
/// Presenter for multiple-choice quizzes. Evaluates selected answers, applies weight penalties,
/// and enforces an optional attempt limit.
/// On a wrong answer, penalises both the asked word and the distractor the user confused it with.
/// </summary>
public class QuizPresenter : IQuizPresenter
{
    private readonly QuizModel _quiz;
    private readonly WordWeightStrategy _weightStrategy;
    private readonly WordListService _wordListService;
    private readonly int? _maxAttempts;
    private int _attemptCount;
    private QuizResult _result = QuizResult.Pending;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuizPresenter"/> class.
    /// </summary>
    /// <param name="quiz">The quiz data for this session.</param>
    /// <param name="weightStrategy">The strategy for updating word weights.</param>
    /// <param name="wordListService">The service used to persist word progress after each answer.</param>
    /// <param name="maxAttempts">Maximum wrong attempts allowed before the quiz ends, or null for unlimited.</param>
    public QuizPresenter(QuizModel quiz, WordWeightStrategy weightStrategy, WordListService wordListService, int? maxAttempts = null)
    {
        _quiz = quiz;
        _weightStrategy = weightStrategy;
        _wordListService = wordListService;
        _maxAttempts = maxAttempts;
    }

    /// <summary>
    /// Evaluates the selected answer. On a correct answer, registers a win and saves.
    /// On a wrong answer, penalises the asked word and the distractor the user chose, then saves.
    /// Has no effect once the quiz has reached a terminal state.
    /// </summary>
    /// <param name="selectedAnswer">The option string the user clicked.</param>
    public void OnAnswerSelected(string selectedAnswer)
    {
        if (_result == QuizResult.Correct || _result == QuizResult.MaxAttemptsReached)
        {
            return;
        }

        _attemptCount++;

        if (selectedAnswer == _quiz.CorrectAnswer)
        {
            _result = QuizResult.Correct;
            _weightStrategy.RegisterCorrect(_quiz.WordEntry);
            _wordListService.SaveWords();
            return;
        }

        // Wrong answer - register mistake on the asked word
        _weightStrategy.RegisterMistake(_quiz.WordEntry);

        // Also penalise the option the user confused it with
        if (_quiz.OptionEntries.TryGetValue(selectedAnswer, out var wrongEntry))
        {
            _weightStrategy.RegisterMistake(wrongEntry);
        }

        _wordListService.SaveWords();

        if (_maxAttempts.HasValue && _attemptCount >= _maxAttempts.Value)
        {
            _result = QuizResult.MaxAttemptsReached;
            return;
        }

        _result = QuizResult.Wrong;
    }

    /// <inheritdoc/>
    public QuizResult GetResult() => _result;

    /// <inheritdoc/>
    public string GetCorrectAnswer() => _quiz.CorrectAnswer;
}
