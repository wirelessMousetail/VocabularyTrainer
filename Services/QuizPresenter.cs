namespace VocabularyTrainer.Services;

public class QuizPresenter : IQuizPresenter
{
    private readonly Quiz _quiz;
    private readonly int? _maxAttempts;
    private int _attemptCount;
    private QuizResult _result = QuizResult.Pending;

    public QuizPresenter(Quiz quiz, int? maxAttempts = null)
    {
        _quiz = quiz;
        _maxAttempts = maxAttempts;
    }

    public void OnAnswerSelected(string selectedAnswer)
    {
        if (_result == QuizResult.Correct || _result == QuizResult.MaxAttemptsReached)
            return;

        _attemptCount++;

        if (selectedAnswer == _quiz.CorrectAnswer)
        {
            _result = QuizResult.Correct;
            return;
        }

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
