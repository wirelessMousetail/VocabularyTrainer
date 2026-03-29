using VocabularyTrainer.Models;

namespace VocabularyTrainer.Services;

public class QuizPresenter : IQuizPresenter
{
    private readonly Quiz _quiz;
    private readonly WordWeightStrategy _weightStrategy;
    private readonly WordListService _wordListService;
    private readonly int? _maxAttempts;
    private int _attemptCount;
    private QuizResult _result = QuizResult.Pending;

    public QuizPresenter(Quiz quiz, WordWeightStrategy weightStrategy, WordListService wordListService, int? maxAttempts = null)
    {
        _quiz = quiz;
        _weightStrategy = weightStrategy;
        _wordListService = wordListService;
        _maxAttempts = maxAttempts;
    }

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

    public QuizResult GetResult() => _result;
    
    public string GetCorrectAnswer() => _quiz.CorrectAnswer;
}
